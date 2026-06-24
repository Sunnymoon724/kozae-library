using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace KZLib.Utilities
{
	/// <summary>Optional acquire parameters forwarded to <see cref="ILane{TPayload}.Initialize"/>.</summary>
	public interface ILaneParam { }

	/// <summary>
	/// Contract for a single lane in <see cref="LanePool{TLane,TPayload}"/>.
	/// </summary>
	/// <typeparam name="TPayload">Payload type held by each lane; exposed as <see cref="Payload"/>.</typeparam>
	/// <remarks>
	/// <see cref="Create"/> binds the lane to its <see cref="Payload"/> and parent storage (typically in a pool <see cref="LanePool{TLane,TPayload}._Create"/> override).
	/// <see cref="Initialize"/> is invoked under the pool lock during <see cref="LanePool{TLane,TPayload}.TryAcquire"/>; keep it lightweight and do not re-enter the pool.
	/// <see cref="Initialize"/> must set <see cref="IsActive"/> to true on acquire and start work so <see cref="IsPlaying"/> becomes true before the next <see cref="LanePool{TLane,TPayload}.Tick"/>.
	/// <see cref="IsPlaying"/> must reflect whether the lane is still executing (e.g. <c>AudioSource.isPlaying</c>).
	/// <see cref="Release"/> must set <see cref="IsActive"/> back to false and be safe to call more than once
	/// (e.g. overlapping <see cref="LanePool{TLane,TPayload}.Tick"/> and explicit release).
	/// </remarks>
	public interface ILane<TPayload>
	{
		/// <summary>Payload bound by <see cref="Create"/>.</summary>
		TPayload Payload { get; }

		/// <summary>True while the pool has checked out this lane. Set in <see cref="Initialize"/>; cleared in <see cref="Release"/>.</summary>
		bool IsActive { get; }
		/// <summary>True while the lane is executing (e.g. playing). Must become true before the next <see cref="LanePool{TLane,TPayload}.Tick"/> and stay accurate while active.</summary>
		bool IsPlaying { get; }

		/// <summary>Creates and binds the lane to <paramref name="payload"/> under <paramref name="storage"/>; assigns <see cref="Payload"/>.</summary>
		/// <param name="payload">Payload instance for this lane.</param>
		/// <param name="storage">Parent transform for lane instances.</param>
		void Create(TPayload payload,Transform storage);

		/// <summary>
		/// Called on acquire under the pool lock. Must set <see cref="IsActive"/> to true, prepare the lane, and start work
		/// so <see cref="IsPlaying"/> becomes true before the next <see cref="LanePool{TLane,TPayload}.Tick"/>.
		/// </summary>
		/// <param name="param">Acquire parameters from <see cref="LanePool{TLane,TPayload}.TryAcquire"/>.</param>
		void Initialize(ILaneParam param);

		/// <summary>Returns the lane to idle. Must clear <see cref="IsActive"/>. Idempotent. Invoked outside the pool lock.</summary>
		void Release();

		/// <summary>Destroys lane resources when the pool is cleared.</summary>
		void Destroy();
	}

	/// <summary>
	/// Bounded lane pool with prepare/max growth. When full and all lanes are active, <see cref="TryAcquire"/> returns false.
	/// </summary>
	/// <typeparam name="TLane">Lane implementation.</typeparam>
	/// <typeparam name="TPayload">Payload type held by each lane.</typeparam>
	/// <remarks>
	/// Pool list access synchronizes on an internal lock.
	/// <see cref="ILane.Initialize"/> runs under that lock during <see cref="TryAcquire"/>; <see cref="ILane.Release"/> and caller predicates/actions run outside it.
	/// Call <see cref="Prepare"/> after construction to avoid creating lanes on the first acquire.
	/// Call <see cref="Tick"/> regularly; finished lanes (active but not playing) are auto-released only there and are not reused until then.
	/// <see cref="Clear"/> releases active lanes, destroys all lanes, and keeps the pool reusable; call <see cref="Prepare"/> again to restore idle lanes.
	/// <see cref="Dispose"/> performs <see cref="_Clear"/> and marks the pool as disposed.
	/// Extend creation/cleanup by overriding <see cref="_Create"/>, <see cref="_Clear"/>, or <see cref="_Dispose(bool)"/>.
	/// </remarks>
	public class LanePool<TLane,TPayload> : IDisposable where TLane : class,ILane<TPayload>,new()
	{
		/// <summary>Synchronizes pool list state. <see cref="ILane.Initialize"/> runs under this lock during <see cref="TryAcquire"/>; <see cref="ILane.Release"/> is delegated outside it through <see cref="Return"/>.</summary>
		private readonly object m_syncRoot = new();
		/// <summary>Number of lanes to pre-create in <see cref="Prepare"/>.</summary>
		private readonly int m_prepareCount = 0;
		/// <summary>Maximum number of lanes; <see cref="TryAcquire"/> fails when full and all are active.</summary>
		private readonly int m_maxCount = 0;

		/// <summary>All lanes created by this pool.</summary>
		protected readonly List<TLane> m_laneList = new();

		/// <summary>Parent transform for lane instances; available to <see cref="_Create"/> overrides.</summary>
		protected readonly Transform m_storage = null!;

		private bool m_disposed = false;

		/// <param name="storage">Parent transform for lane instances.</param>
		/// <param name="prepareCount">Lanes to create in <see cref="Prepare"/> (default 8).</param>
		/// <param name="maxCount">Upper bound on lane count (default 32).</param>
		public LanePool(Transform storage,int prepareCount = 8,int maxCount = 32)
		{
			if(prepareCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(prepareCount),prepareCount,"Prepare count cannot be negative.");
			}

			if(maxCount <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxCount),maxCount,"Max count must be greater than zero.");
			}

			if(prepareCount > maxCount)
			{
				throw new ArgumentOutOfRangeException(nameof(prepareCount),prepareCount,"Prepare count cannot exceed max count.");
			}

			m_storage = storage;

			m_prepareCount = prepareCount;
			m_maxCount = maxCount;
		}

		/// <summary>Total number of lanes created so far (idle + active).</summary>
		public int Count
		{
			get
			{
				lock(m_syncRoot)
				{
					_EnsureNotDisposed();

					return m_laneList.Count;
				}
			}
		}

		/// <summary>Number of lanes with <see cref="ILane.IsActive"/> true.</summary>
		public int ActiveCount
		{
			get
			{
				lock(m_syncRoot)
				{
					_EnsureNotDisposed();

					var count = 0;

					for(var i=0;i<m_laneList.Count;i++)
					{
						if(m_laneList[i].IsActive)
						{
							count++;
						}
					}

					return count;
				}
			}
		}

		/// <summary>Pre-creates idle lanes up to the configured prepare count.</summary>
		public void Prepare()
		{
			lock(m_syncRoot)
			{
				_EnsureNotDisposed();

				for(var i=m_laneList.Count;i<m_prepareCount;i++)
				{
					m_laneList.Add(_Create(i));
				}
			}
		}

		/// <summary>Destroys all lanes via <see cref="_Clear"/> and marks the pool as disposed.</summary>
		public void Dispose()
		{
			_Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void _Dispose(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				_Clear();
			}

			m_disposed = true;
		}

		/// <summary>
		/// Borrows an idle lane and activates it via <see cref="ILane.Initialize"/>, or creates one when below <see cref="m_maxCount"/>.
		/// <paramref name="param"/> is forwarded to <see cref="ILane.Initialize"/>, which runs under the pool lock; start work there so <see cref="ILane.IsPlaying"/> becomes true.
		/// Finished active lanes are not reclaimed or reused here; call <see cref="Tick"/> to return them when they stop playing.
		/// </summary>
		/// <param name="param">Acquire parameters forwarded to <see cref="ILane.Initialize"/>.</param>
		/// <returns>False when the pool is at max capacity and every lane is active.</returns>
		public bool TryAcquire(ILaneParam param,[NotNullWhen(true)] out TLane? lane)
		{
			lock(m_syncRoot)
			{
				_EnsureNotDisposed();

				for(var i=0;i<m_laneList.Count;i++)
				{
					var candidate = m_laneList[i];

					if(!candidate.IsActive)
					{
						_ActivateLane(candidate,param);

						lane = candidate;

						return true;
					}
				}

				if(m_laneList.Count < m_maxCount)
				{
					var newLane = _Create(m_laneList.Count);

					m_laneList.Add(newLane);

					_ActivateLane(newLane,param);

					lane = newLane;

					return true;
				}

				lane = null;

				return false;
			}
		}

		/// <summary>
		/// Scans active lanes and returns those that are no longer playing (<see cref="ILane.IsActive"/> and not <see cref="ILane.IsPlaying"/>).
		/// Does not drive lane execution; <see cref="ILane.IsPlaying"/> must reflect completion (e.g. <c>AudioSource.isPlaying</c>).
		/// Release is routed through <see cref="Return"/>, which performs the active check under the pool lock and calls <see cref="ILane.Release"/> outside it.
		/// </summary>
		public void Tick()
		{
			_EnsureNotDisposed();

			var activeLaneList = new List<TLane>();

			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var lane = m_laneList[i];

					if(lane.IsActive)
					{
						activeLaneList.Add(lane);
					}
				}
			}

			for(var i=0;i<activeLaneList.Count;i++)
			{
				var lane = activeLaneList[i];

				if(!_ShouldRelease(lane))
				{
					continue;
				}

				Return(lane);
			}
		}

		/// <summary>Returns the first lane that satisfies <paramref name="onPredicate"/>. Snapshots the lane list under the pool lock; <paramref name="onPredicate"/> runs outside it.</summary>
		public bool TryFind(Func<TLane,bool> onPredicate,[NotNullWhen(true)] out TLane? lane)
		{
			if(onPredicate == null)
			{
				throw new ArgumentNullException(nameof(onPredicate));
			}

			_EnsureNotDisposed();

			var laneList = new List<TLane>();

			lock(m_syncRoot)
			{
				laneList.AddRange(m_laneList);
			}

			for(var i=0;i<laneList.Count;i++)
			{
				var candidate = laneList[i];

				if(onPredicate(candidate))
				{
					lane = candidate;

					return true;
				}
			}

			lane = null;

			return false;
		}

		/// <summary>Returns the first active lane that satisfies <paramref name="onPredicate"/>. Snapshots active lanes under the pool lock; <paramref name="onPredicate"/> runs outside it.</summary>
		public bool TryFindActive(Func<TLane,bool> onPredicate,[NotNullWhen(true)] out TLane? lane)
		{
			if(onPredicate == null)
			{
				throw new ArgumentNullException(nameof(onPredicate));
			}

			_EnsureNotDisposed();

			var activeLaneList = new List<TLane>();

			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var candidate = m_laneList[i];

					if(!candidate.IsActive)
					{
						continue;
					}

					activeLaneList.Add(candidate);
				}
			}

			for(var i=0;i<activeLaneList.Count;i++)
			{
				var candidate = activeLaneList[i];

				if(onPredicate(candidate))
				{
					lane = candidate;

					return true;
				}
			}

			lane = null;

			return false;
		}

		/// <summary>Invokes <paramref name="onAction"/> for each active lane. Snapshots under the pool lock; <paramref name="onAction"/> runs outside it. Do not call pool mutators from <paramref name="onAction"/>.</summary>
		public void ForEachActive(Action<TLane> onAction)
		{
			if(onAction == null)
			{
				throw new ArgumentNullException(nameof(onAction));
			}

			_EnsureNotDisposed();

			var activeLaneList = new List<TLane>();

			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var lane = m_laneList[i];

					if(lane.IsActive)
					{
						activeLaneList.Add(lane);
					}
				}
			}

			for(var i=0;i<activeLaneList.Count;i++)
			{
				onAction(activeLaneList[i]);
			}
		}

		/// <summary>
		/// Explicitly returns a lane to idle. Reads <see cref="ILane.IsActive"/> under the pool lock; delegates to <see cref="ILane.Release"/> outside it.
		/// External callers may invoke <see cref="ILane.Release"/> directly, but <see cref="Return"/> preserves the pool's disposed guard and active-state check.
		/// </summary>
		public void Return(TLane lane)
		{
			if(lane == null)
			{
				return;
			}

			_EnsureNotDisposed();

			var shouldRelease = false;

			lock(m_syncRoot)
			{
				shouldRelease = lane.IsActive;
			}

			if(shouldRelease)
			{
				lane.Release();
			}
		}

		/// <summary>Releases the first active lane that satisfies <paramref name="onPredicate"/>.</summary>
		/// <returns>True when a matching lane was released.</returns>
		public bool TryReleaseActive(Func<TLane,bool> onPredicate)
		{
			if(onPredicate == null)
			{
				throw new ArgumentNullException(nameof(onPredicate));
			}

			_EnsureNotDisposed();

			if(!TryFindActive(onPredicate,out var lane))
			{
				return false;
			}

			Return(lane);

			return true;
		}

		/// <summary>Releases every active lane that satisfies <paramref name="onPredicate"/>.</summary>
		/// <returns>True when at least one matching lane was released.</returns>
		public bool TryReleaseAllActive(Func<TLane,bool> onPredicate)
		{
			if(onPredicate == null)
			{
				throw new ArgumentNullException(nameof(onPredicate));
			}

			_EnsureNotDisposed();

			var activeLaneList = new List<TLane>();

			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var lane = m_laneList[i];

					if(!lane.IsActive)
					{
						continue;
					}

					activeLaneList.Add(lane);
				}
			}

			var targetLaneList = new List<TLane>();

			for(var i=0;i<activeLaneList.Count;i++)
			{
				var activeLane = activeLaneList[i];

				if(!onPredicate(activeLane))
				{
					continue;
				}

				targetLaneList.Add(activeLane);
			}

			for(var i=0;i<targetLaneList.Count;i++)
			{
				var targetLane = targetLaneList[i];

				Return(targetLane);
			}

			return targetLaneList.Count > 0;
		}

		/// <summary>Releases every lane that is currently active.</summary>
		public void ReleaseAll()
		{
			_EnsureNotDisposed();

			var activeLaneList = new List<TLane>();

			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var lane = m_laneList[i];

					if(!lane.IsActive)
					{
						continue;
					}

					activeLaneList.Add(lane);
				}
			}

			if(activeLaneList.Count == 0)
			{
				return;
			}

			for(var i=0;i<activeLaneList.Count;i++)
			{
				Return(activeLaneList[i]);
			}
		}

		/// <summary>Releases active lanes, destroys all lanes, and empties the pool. Call <see cref="Prepare"/> again to restore pre-warmed idle lanes (optional).</summary>
		public void Clear()
		{
			_EnsureNotDisposed();

			_Clear();
		}

		/// <summary>Releases active lanes, destroys all lanes, and empties the pool without checking disposal state. Used by <see cref="Dispose"/>.</summary>
		protected virtual void _Clear()
		{
			var laneList = new List<TLane>();
			var activeLaneList = new List<TLane>();

			lock(m_syncRoot)
			{
				laneList.AddRange(m_laneList);

				for(var i=0;i<laneList.Count;i++)
				{
					var lane = laneList[i];

					if(!lane.IsActive)
					{
						continue;
					}

					activeLaneList.Add(lane);
				}

				m_laneList.Clear();
			}

			if(activeLaneList.Count != 0)
			{
				for(var i=0;i<activeLaneList.Count;i++)
				{
					Return(activeLaneList[i]);
				}
			}

			for(var i=0;i<laneList.Count;i++)
			{
				laneList[i].Destroy();
			}
		}

		/// <summary>Activates a lane via <see cref="ILane.Initialize"/>. Must be called under <see cref="m_syncRoot"/>.</summary>
		private void _ActivateLane(TLane lane,ILaneParam param)
		{
			lane.Initialize(param);
		}

		/// <summary>Finished lane: active but no longer playing. Also covers acquire-without-start.</summary>
		protected bool _ShouldRelease(TLane lane)
		{
			return lane.IsActive && !lane.IsPlaying;
		}

		/// <summary>Creates a new lane. Override to provision the instance and call <see cref="ILane{TPayload}.Create"/> with payload and <see cref="m_storage"/>.</summary>
		/// <param name="index">Zero-based lane index within this pool.</param>
		protected virtual TLane _Create(int index)
		{
			var lane = new TLane();

			return lane;
		}

		private void _EnsureNotDisposed()
		{
			if(m_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}