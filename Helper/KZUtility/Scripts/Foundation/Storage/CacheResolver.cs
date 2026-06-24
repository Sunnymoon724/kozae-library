using System.Collections.Generic;
using System;
using System.Threading;

namespace KZLib.Utilities
{
	/// <summary>
	/// TTL string-key cache with a FIFO queue per key. Expired entries are dropped on dequeue or by an optional background timer.
	/// </summary>
	/// <typeparam name="TCache">Cached payload type.</typeparam>
	/// <remarks>
	/// Dictionary access synchronizes on an internal lock. The purge timer callback also runs under that lock.
	/// <see cref="TryGetCache"/> is consumptive: it dequeues the oldest non-expired entry and does not peek or retain it.
	/// <see cref="StoreCache"/> appends a new entry. When <c>isUpdate</c> is true, every pending entry for the same key receives a new expiration before the new one is enqueued.
	/// Expired payloads are removed from this cache only; tearing down <typeparamref name="TCache"/> itself is the caller's responsibility.
	/// <see cref="Clear"/> removes all cached entries while keeping the purge timer active and the resolver reusable.
	/// <see cref="Dispose"/> stops the purge timer, waits for any in-flight callback, clears all queues, and marks the resolver as disposed.
	/// This type is <c>sealed</c>, so cleanup uses a private <see cref="_Dispose"/> rather than a <c>protected virtual</c> hook.
	/// </remarks>
	public sealed class CacheResolver<TCache> : IDisposable
	{
		/// <summary>Payload plus an absolute UTC expiration tick (<see cref="DateTime.Ticks"/>).</summary>
		private record CacheInfo
		{
			/// <summary>Stored payload.</summary>
			public TCache Cache { get; }

			/// <summary>True when <see cref="DateTime.UtcNow"/> has reached or passed the stored expiration tick.</summary>
			public bool IsOverdue => m_expiresAtTicks <= DateTime.UtcNow.Ticks;

			/// <summary>Absolute expiration in UTC ticks.</summary>
			private long m_expiresAtTicks = 0L;

			/// <param name="cache">Payload to store.</param>
			/// <param name="expiresAtTicks">Absolute expiration in UTC ticks.</param>
			public CacheInfo(TCache cache,long expiresAtTicks)
			{
				Cache = cache;
				m_expiresAtTicks = expiresAtTicks;
			}

			/// <summary>Replaces the absolute expiration tick for this entry.</summary>
			/// <param name="expiresAtTicks">New absolute expiration in UTC ticks.</param>
			public void UpdateExpiration(long expiresAtTicks)
			{
				m_expiresAtTicks = expiresAtTicks;
			}
		}

		/// <summary>Synchronizes dictionary and queue state. The purge timer callback acquires this lock.</summary>
		private readonly object m_syncRoot = new();
		/// <summary>FIFO queues of cached entries keyed by caller-supplied string.</summary>
		private readonly Dictionary<string,Queue<CacheInfo>> m_cacheQueueDict = new();
		/// <summary>Scratch list reused by <see cref="_PurgeOverdueCaches"/> to collect keys whose queues became empty.</summary>
		private readonly List<string> m_removeList = new();

		/// <summary>Background purge timer. Null when <see cref="m_updatePeriod"/> is zero.</summary>
		private Timer? m_timer = null;

		/// <summary>Entry lifetime in seconds from the moment it is stored.</summary>
		private readonly float m_deleteTime = 0.0f;
		/// <summary>Seconds between background purge runs. Zero disables the purge timer.</summary>
		private readonly float m_updatePeriod = 0.0f;

		private bool m_disposed = false;

		/// <summary>
		/// Creates a cache resolver and optionally starts a timer that periodically removes overdue entries.
		/// </summary>
		/// <param name="deleteTime">Seconds until a stored entry expires.</param>
		/// <param name="updatePeriod">Seconds between background purge runs. Zero disables the purge timer.</param>
		public CacheResolver(float deleteTime = 60.0f,float updatePeriod = 30.0f)
		{
			if(deleteTime < 0.0f)
			{
				throw new ArgumentOutOfRangeException(nameof(deleteTime),deleteTime,"Delete time cannot be negative.");
			}

			if(updatePeriod < 0.0f)
			{
				throw new ArgumentOutOfRangeException(nameof(updatePeriod),updatePeriod,"Update period cannot be negative.");
			}

			m_deleteTime = deleteTime;
			m_updatePeriod = updatePeriod;

			if(m_updatePeriod > 0.0f)
			{
				var period = TimeSpan.FromSeconds(m_updatePeriod);

				m_timer = new Timer(_OnTimer,null,TimeSpan.Zero,period);
			}
		}

		/// <summary>Stops the purge timer, clears all cached entries, and marks the resolver as disposed.</summary>
		public void Dispose()
		{
			_Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Stops the purge timer and clears cached entries when disposing.</summary>
		/// <param name="disposing">When true, stops the timer and clears all queues.</param>
		private void _Dispose(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				if(m_timer != null)
				{
					using var waitHandle = new ManualResetEvent(false);

					if(!m_timer.Dispose(waitHandle))
					{
						waitHandle.WaitOne();
					}

					m_timer = null;
				}

				_Clear();
			}

			m_disposed = true;
		}

		/// <summary>Removes all cached entries. The purge timer stays active and the resolver remains usable.</summary>
		public void Clear()
		{
			_EnsureNotDisposed();

			_Clear();
		}

		/// <summary>Clears all queues under <see cref="m_syncRoot"/>.</summary>
		private void _Clear()
		{
			lock(m_syncRoot)
			{
				m_cacheQueueDict.Clear();
				m_removeList.Clear();
			}
		}

		/// <summary>
		/// Dequeues the oldest non-expired cache for <paramref name="key"/>.
		/// Expired entries at the front of the queue are skipped and dropped.
		/// </summary>
		/// <param name="key">Cache key. Must not be null.</param>
		/// <param name="cache">Dequeued payload when this method returns true.</param>
		/// <returns>True when a valid cache was returned; false when the key is missing or all entries are expired.</returns>
		public bool TryGetCache(string key,out TCache cache)
		{
			_EnsureNotDisposed();
			_ValidateKey(key);

			lock(m_syncRoot)
			{
				if(!m_cacheQueueDict.TryGetValue(key,out var cacheQueue) || cacheQueue.Count == 0)
				{
					cache = default!;

					return false;
				}

				while(cacheQueue.Count > 0)
				{
					var cacheInfo = cacheQueue.Dequeue();

					if(cacheInfo.IsOverdue)
					{
						continue;
					}

					if(cacheQueue.Count == 0)
					{
						m_cacheQueueDict.Remove(key);
					}

					cache = cacheInfo.Cache;

					return true;
				}

				m_cacheQueueDict.Remove(key);
				cache = default!;

				return false;
			}
		}

		/// <summary>
		/// Enqueues a cache entry for <paramref name="key"/>.
		/// When <paramref name="isUpdate"/> is true, extends the expiration of all pending entries for the same key before enqueueing the new one.
		/// </summary>
		/// <param name="key">Cache key. Must not be null.</param>
		/// <param name="cache">Payload to store.</param>
		/// <param name="isUpdate">When true, refreshes the expiration of every pending entry for <paramref name="key"/> before enqueueing <paramref name="cache"/>.</param>
		public void StoreCache(string key,TCache cache,bool isUpdate)
		{
			_EnsureNotDisposed();
			_ValidateKey(key);

			lock(m_syncRoot)
			{
				var expiresAtTicks = DateTime.UtcNow.AddSeconds(m_deleteTime).Ticks;

				if(!m_cacheQueueDict.TryGetValue(key,out var cacheQueue))
				{
					cacheQueue = new Queue<CacheInfo>();

					m_cacheQueueDict.Add(key,cacheQueue);
				}
				else if(isUpdate && cacheQueue.Count > 0)
				{
					var pendingCount = cacheQueue.Count;

					for(var i=0;i<pendingCount;i++)
					{
						var cacheInfo = cacheQueue.Dequeue();

						cacheInfo.UpdateExpiration(expiresAtTicks);
						cacheQueue.Enqueue(cacheInfo);
					}
				}

				cacheQueue.Enqueue(new CacheInfo(cache,expiresAtTicks));
			}
		}

		/// <summary>Throws <see cref="ArgumentNullException"/> when <paramref name="key"/> is null.</summary>
		private static void _ValidateKey(string key)
		{
			if(key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}
		}

		/// <summary>Throws <see cref="ObjectDisposedException"/> when the resolver has been disposed.</summary>
		private void _EnsureNotDisposed()
		{
			if(m_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		/// <summary>Timer callback that purges expired entries from all keys under <see cref="m_syncRoot"/>.</summary>
		private void _OnTimer(object? state)
		{
			if(m_disposed)
			{
				return;
			}

			lock(m_syncRoot)
			{
				if(m_disposed)
				{
					return;
				}

				_PurgeOverdueCaches();
			}
		}

		/// <summary>
		/// Removes expired entries in place and drops keys whose queues become empty.
		/// Must be called while holding <see cref="m_syncRoot"/>.
		/// </summary>
		private void _PurgeOverdueCaches()
		{
			m_removeList.Clear();

			foreach(var pair in m_cacheQueueDict)
			{
				var cacheQueue = pair.Value;
				var pendingCount = cacheQueue.Count;

				for(var i=0;i<pendingCount;i++)
				{
					var cacheInfo = cacheQueue.Dequeue();

					if(!cacheInfo.IsOverdue)
					{
						cacheQueue.Enqueue(cacheInfo);
					}
				}

				if(cacheQueue.Count == 0)
				{
					m_removeList.Add(pair.Key);
				}
			}

			for(var i=0;i<m_removeList.Count;i++)
			{
				m_cacheQueueDict.Remove(m_removeList[i]);
			}
		}
	}
}
