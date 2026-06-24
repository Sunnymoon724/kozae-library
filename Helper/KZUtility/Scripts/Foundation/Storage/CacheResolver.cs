using System.Collections.Generic;
using System;
using System.Threading;

namespace KZLib.Utilities
{
	/// <summary>
	/// Time-based cache keyed by string. Each key can hold multiple entries in FIFO order.
	/// Entries expire after the configured lifetime and are purged on access or by a background timer.
	/// </summary>
	/// <typeparam name="TCache">Cached payload type.</typeparam>
	public sealed class CacheResolver<TCache> : IDisposable
	{
		/// <summary>
		/// Wraps a cached value with an absolute expiration tick (<see cref="DateTime.Ticks"/>).
		/// </summary>
		private record CacheInfo
		{
			public TCache Cache { get; }
			public bool IsOverdue => m_duration < DateTime.UtcNow.Ticks;

			private long m_duration = 0L;

			public CacheInfo(TCache cache,long duration)
			{
				Cache = cache;
				m_duration = duration;
			}

			/// <summary>
			/// Extends the expiration of this entry.
			/// </summary>
			public void UpdateDuration(long duration)
			{
				m_duration = duration;
			}
		}

		private readonly object m_syncRoot = new();
		private readonly Dictionary<string,List<CacheInfo>> m_cacheInfoListDict = new();
		private readonly List<string> m_removeList = new();

		private Timer? m_timer = null;

		/// <summary>Entry lifetime in seconds.</summary>
		private readonly float m_deleteTime = 0.0f;
		/// <summary>Background purge interval in seconds.</summary>
		private readonly float m_updatePeriod = 0.0f;

		private bool m_disposed = false;

		/// <summary>
		/// Creates a cache resolver and starts a timer that periodically removes overdue entries.
		/// </summary>
		/// <param name="deleteTime">Seconds until a stored entry expires.</param>
		/// <param name="updatePeriod">Seconds between background purge runs.</param>
		public CacheResolver(float deleteTime = 60.0f,float updatePeriod = 30.0f)
		{
			m_deleteTime = deleteTime;
			m_updatePeriod = updatePeriod;

			var period = TimeSpan.FromSeconds(m_updatePeriod);

			m_timer = new Timer(_OnTimer,null,TimeSpan.Zero,period);
		}

		/// <summary>
		/// Stops the purge timer and clears all cached entries.
		/// </summary>
		public void Dispose()
		{
			_Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void _Dispose(bool disposing)
		{
			if(m_disposed || !disposing)
			{
				return;
			}

			m_disposed = true;

			if(m_timer != null)
			{
				using var waitHandle = new ManualResetEvent(false);

				if(!m_timer.Dispose(waitHandle))
				{
					waitHandle.WaitOne();
				}

				m_timer = null;
			}

			lock(m_syncRoot)
			{
				m_cacheInfoListDict.Clear();
				m_removeList.Clear();
			}
		}

		/// <summary>
		/// Returns the oldest non-expired cache for <paramref name="key"/> and removes it from the queue.
		/// Skips expired entries at the front of the queue.
		/// </summary>
		/// <returns>True when a valid cache was returned; false when the key is missing or all entries are expired.</returns>
		public bool TryGetCache(string key,out TCache cache)
		{
			_EnsureNotDisposed();

			lock(m_syncRoot)
			{
				if(!m_cacheInfoListDict.TryGetValue(key,out var cacheInfoList) || cacheInfoList.Count == 0)
				{
					cache = default!;

					return false;
				}

				while(cacheInfoList.Count > 0)
				{
					var cacheInfo = cacheInfoList[0];
					cacheInfoList.RemoveAt(0);

					if(cacheInfo.IsOverdue)
					{
						continue;
					}

					if(cacheInfoList.Count == 0)
					{
						m_cacheInfoListDict.Remove(key);
					}

					cache = cacheInfo.Cache;

					return true;
				}

				m_cacheInfoListDict.Remove(key);
				cache = default!;

				return false;
			}
		}

		/// <summary>
		/// Appends a cache entry for <paramref name="key"/>.
		/// When <paramref name="isUpdate"/> is true, extends the expiration of existing entries for the same key before adding the new one.
		/// </summary>
		public void StoreCache(string key,TCache cache,bool isUpdate)
		{
			_EnsureNotDisposed();

			lock(m_syncRoot)
			{
				var currentTime = DateTime.UtcNow;
				var newDuration = currentTime.AddSeconds(m_deleteTime).Ticks;

				if(!m_cacheInfoListDict.TryGetValue(key,out var cacheInfoList))
				{
					cacheInfoList = new List<CacheInfo>();

					m_cacheInfoListDict.Add(key,cacheInfoList);
				}
				else if(isUpdate)
				{
					for(var i=0;i<cacheInfoList.Count;i++)
					{
						cacheInfoList[i].UpdateDuration(newDuration);
					}
				}

				cacheInfoList.Add(new CacheInfo(cache,newDuration));
			}
		}

		private void _EnsureNotDisposed()
		{
			if(m_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		/// <summary>
		/// Timer callback that purges expired entries from all keys.
		/// </summary>
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
		/// Removes expired entries in place and drops keys whose lists become empty.
		/// Must be called while holding <see cref="m_syncRoot"/>.
		/// </summary>
		private void _PurgeOverdueCaches()
		{
			m_removeList.Clear();

			foreach(var pair in m_cacheInfoListDict)
			{
				pair.Value.RemoveAll(static cacheInfo => cacheInfo.IsOverdue);

				if(pair.Value.Count == 0)
				{
					m_removeList.Add(pair.Key);
				}
			}

			for(var i=0;i<m_removeList.Count;i++)
			{
				m_cacheInfoListDict.Remove(m_removeList[i]);
			}
		}
	}
}