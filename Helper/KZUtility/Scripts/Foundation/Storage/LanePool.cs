using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KZLib.Utilities
{
	/// <summary>
	/// Contract for a single lane in <see cref="LanePool{TLane}"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="IsActive"/> is set to true by the pool on acquire; <see cref="Release"/> must set it back to false.
	/// Start work immediately after acquire so <see cref="IsPlaying"/> becomes true; otherwise the pool may auto-release the lane.
	/// </remarks>
	public interface ILane
	{
		/// <summary>True while the pool has checked out this lane.</summary>
		bool IsActive { get; set; }
		/// <summary>True while the lane is executing (e.g. playing). Must become true right after acquire.</summary>
		bool IsPlaying { get; }
		/// <summary>Advances lane state. Invoked by <see cref="LanePool{TLane}.Tick"/> outside the pool lock.</summary>
		void Tick();
		/// <summary>Returns the lane to idle. Must clear <see cref="IsActive"/>. Invoked outside the pool lock.</summary>
		void Release();
		/// <summary>Destroys lane resources when the pool is cleared.</summary>
		void Destroy();
	}

	/// <summary>
	/// Fixed lanes with prepare/max growth. When full, the oldest active lane is released and reused.
	/// List order tracks age: front is oldest, back is newest.
	/// </summary>
	/// <typeparam name="TLane">Lane implementation.</typeparam>
	/// <remarks>
	/// Thread-safe; pool state synchronizes on an internal lock. <see cref="ILane.Release"/> runs outside that lock.
	/// Call <see cref="Prepare"/> after construction to avoid creating lanes on the first acquire.
	/// Callers should start the lane immediately after <see cref="TryAcquire"/>; holding an active lane without <see cref="ILane.IsPlaying"/> may be released automatically.
	/// </remarks>
	public sealed class LanePool<TLane> where TLane : class,ILane
	{
		/// <summary>Synchronizes pool list state. <see cref="ILane.Release"/> is not called under this lock.</summary>
		private readonly object m_syncRoot = new();
		/// <summary>Factory invoked with a zero-based lane index when a new slot is created.</summary>
		private readonly Func<int,TLane> m_onCreateFunc = null!;
		/// <summary>Number of lanes to pre-create in <see cref="Prepare"/>.</summary>
		private readonly int m_prepareCount = 0;
		/// <summary>Maximum number of lanes; oldest active lane is reclaimed when full.</summary>
		private readonly int m_maxCount = 0;

		/// <summary>All lanes. Front is oldest, back is newest (see <see cref="_ActivateLane"/>).</summary>
		private readonly List<TLane> m_laneList = new();
		/// <summary>Lanes mid-reclaim during steal; excluded from acquire and auto-release.</summary>
		private readonly HashSet<TLane> m_reclaimingSet = new();

		/// <param name="onCreateFunc">Creates a lane for the given index.</param>
		/// <param name="prepareCount">Lanes to create in <see cref="Prepare"/> (default 8).</param>
		/// <param name="maxCount">Upper bound on lane count (default 32).</param>
		public LanePool(Func<int,TLane> onCreateFunc,int prepareCount = 8,int maxCount = 32)
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

			m_onCreateFunc = onCreateFunc ?? throw new ArgumentNullException(nameof(onCreateFunc));
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
				for(var i=m_laneList.Count;i<m_prepareCount;i++)
				{
					m_laneList.Add(m_onCreateFunc(i));
				}
			}
		}

		/// <summary>
		/// Borrows a lane and marks it active. When at max capacity, releases the oldest active lane and reuses it.
		/// Releases finished lanes (active but not playing) while scanning for a free slot.
		/// Caller must start the lane immediately so <see cref="ILane.IsPlaying"/> becomes true.
		/// </summary>
		/// <returns>False only when the pool is at max capacity and no lane can be reclaimed.</returns>
		public bool TryAcquire([NotNullWhen(true)] out TLane? lane)
		{
			List<TLane>? toRelease = null;

			// Phase 1: collect finished lanes under lock, release outside lock.
			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var candidate = m_laneList[i];

					if(_ShouldRelease(candidate))
					{
						toRelease ??= new List<TLane>();
						toRelease.Add(candidate);
					}
				}
			}

			if(toRelease != null)
			{
				for(var i=0;i<toRelease.Count;i++)
				{
					toRelease[i].Release();
				}
			}

			TLane? reclaimLane = null;

			// Phase 2: idle slot, grow, or mark steal victim.
			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var candidate = m_laneList[i];

					if(m_reclaimingSet.Contains(candidate))
					{
						continue;
					}

					if(!candidate.IsActive)
					{
						_ActivateLane(candidate,i);
						lane = candidate;

						return true;
					}
				}

				if(m_laneList.Count < m_maxCount)
				{
					var newLane = m_onCreateFunc(m_laneList.Count);

					m_laneList.Add(newLane);
					_ActivateLane(newLane,m_laneList.Count-1);
					lane = newLane;

					return true;
				}

				if(!_TryFindOldestActiveLane(out reclaimLane,out _))
				{
					lane = null;

					return false;
				}

				m_reclaimingSet.Add(reclaimLane);
			}

			reclaimLane!.Release();

			// Phase 3: re-activate stolen lane under lock.
			lock(m_syncRoot)
			{
				m_reclaimingSet.Remove(reclaimLane);

				var index = m_laneList.IndexOf(reclaimLane);

				if(index < 0)
				{
					lane = null;

					return false;
				}

				_ActivateLane(reclaimLane,index);
				lane = reclaimLane;

				return true;
			}
		}

		/// <summary>
		/// Updates active lanes and releases those that are no longer playing.
		/// Lane <see cref="ILane.Tick"/> and <see cref="ILane.Release"/> run outside the pool lock.
		/// </summary>
		public void Tick()
		{
			List<TLane>? snapshot = null;

			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var lane = m_laneList[i];

					if(!lane.IsActive)
					{
						continue;
					}

					snapshot ??= new List<TLane>();
					snapshot.Add(lane);
				}
			}

			if(snapshot == null)
			{
				return;
			}

			for(var i=0;i<snapshot.Count;i++)
			{
				var lane = snapshot[i];

				lane.Tick();

				var shouldRelease = false;

				lock(m_syncRoot)
				{
					shouldRelease = _ShouldRelease(lane);
				}

				if(shouldRelease)
				{
					lane.Release();
				}
			}
		}

		/// <summary>Explicitly releases a lane. Delegates to <see cref="ILane.Release"/> outside the pool lock.</summary>
		public void ReleaseLane(TLane lane)
		{
			if(lane == null)
			{
				return;
			}

			lane.Release();
		}

		/// <summary>Releases every lane that is currently active.</summary>
		public void ReleaseAll()
		{
			List<TLane>? activeLanes = null;

			lock(m_syncRoot)
			{
				for(var i=0;i<m_laneList.Count;i++)
				{
					var lane = m_laneList[i];

					if(!lane.IsActive)
					{
						continue;
					}

					activeLanes ??= new List<TLane>();
					activeLanes.Add(lane);
				}
			}

			if(activeLanes == null)
			{
				return;
			}

			for(var i=0;i<activeLanes.Count;i++)
			{
				activeLanes[i].Release();
			}
		}

		/// <summary>Destroys all lanes and empties the pool. Call <see cref="Prepare"/> before acquiring again.</summary>
		public void Clear()
		{
			TLane[] lanes;
			List<TLane>? activeLanes = null;

			lock(m_syncRoot)
			{
				lanes = m_laneList.ToArray();

				for(var i=0;i<lanes.Length;i++)
				{
					var lane = lanes[i];

					if(!lane.IsActive)
					{
						continue;
					}

					activeLanes ??= new List<TLane>();
					activeLanes.Add(lane);
				}

				m_laneList.Clear();
				m_reclaimingSet.Clear();
			}

			if(activeLanes != null)
			{
				for(var i=0;i<activeLanes.Count;i++)
				{
					activeLanes[i].Release();
				}
			}

			for(var i=0;i<lanes.Length;i++)
			{
				lanes[i].Destroy();
			}
		}

		/// <summary>Invokes <paramref name="onLane"/> for each lane using a snapshot; safe to call pool members from the callback.</summary>
		public void ForEach(Action<TLane> onLane)
		{
			if(onLane == null)
			{
				throw new ArgumentNullException(nameof(onLane));
			}

			TLane[] snapshot;

			lock(m_syncRoot)
			{
				snapshot = m_laneList.ToArray();
			}

			for(var i=0;i<snapshot.Length;i++)
			{
				onLane(snapshot[i]);
			}
		}

		/// <summary>Marks a lane active and moves it to the end of <see cref="m_laneList"/> (newest).</summary>
		/// <param name="index">Current index in <see cref="m_laneList"/>; skip reorder when already last.</param>
		private void _ActivateLane(TLane lane,int index)
		{
			// Pool marks the lane checked-out; ILane.Release must clear IsActive.
			lane.IsActive = true;

			if(index >= 0 && index < m_laneList.Count-1)
			{
				m_laneList.RemoveAt(index);
				m_laneList.Add(lane);
			}
		}

		/// <summary>Finished or idle-active lane: active but no longer playing. Intentionally includes acquire-without-start (caller bug).</summary>
		private bool _ShouldRelease(TLane lane)
		{
			return lane.IsActive && !lane.IsPlaying && !m_reclaimingSet.Contains(lane);
		}

		/// <summary>Front of the list is oldest. Prefers a playing lane; falls back to any active lane (e.g. acquired but not started).</summary>
		private bool _TryFindOldestActiveLane([NotNullWhen(true)] out TLane? lane,out int index)
		{
			TLane? fallback = null;
			var fallbackIndex = -1;

			for(var i=0;i<m_laneList.Count;i++)
			{
				var candidate = m_laneList[i];

				if(!candidate.IsActive || m_reclaimingSet.Contains(candidate))
				{
					continue;
				}

				if(candidate.IsPlaying)
				{
					lane = candidate;
					index = i;

					return true;
				}

				if(fallback == null)
				{
					fallback = candidate;
					fallbackIndex = i;
				}
			}

			if(fallback != null)
			{
				lane = fallback;
				index = fallbackIndex;

				return true;
			}

			lane = null;
			index = -1;

			return false;
		}
	}
}
