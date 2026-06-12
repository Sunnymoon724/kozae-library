using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	/// <summary>
	/// Thread-safe generational slot map that issues stable integer handles for stored values.
	/// </summary>
	/// <typeparam name="TValue">Stored payload type.</typeparam>
	/// <remarks>
	/// Handles pack a 16-bit slot index and a 16-bit generation into one <see langword="int"/>.
	/// Up to 65,536 concurrent live entries and 65,535 reuses per slot are supported.
	/// <see cref="Create"/> never returns <c>0</c>. Use <c>0</c> in consuming code to mean an unset handle,
	/// and skip <see cref="TryGet"/>, <see cref="Get"/>, and <see cref="Destroy"/> when the handle is <c>0</c>.
	/// </remarks>
	public sealed class SlotMap<TValue>
	{
		private const int c_indexBits = 16;
		private const int c_indexMask = 0xFFFF;
		private const int c_maxIndex = c_indexMask;
		private const int c_maxGeneration = c_indexMask;

		private struct Slot
		{
			public int Generation;
			public bool IsAlive;
			public TValue Value;
		}

		private readonly List<Slot> m_slotList = new();
		private readonly Stack<int> m_freeIndexStack = new();
		private readonly object m_syncRoot = new();

		private int m_aliveCount = 0;

		/// <summary>Number of live entries.</summary>
		public int Count
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_aliveCount;
				}
			}
		}

		/// <summary>Maximum number of live entries supported by the packed handle format.</summary>
		public static int MaxCapacity => c_maxIndex+1;

		/// <summary>
		/// Allocates a slot, stores <paramref name="val"/>, and returns a new handle.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the map is at capacity or a slot generation overflows.</exception>
		public int Create(TValue val)
		{
			lock(m_syncRoot)
			{
				if(!_TryCreate(val,out var id))
				{
					throw new InvalidOperationException("Slot map is at capacity or every reusable slot reached the maximum generation.");
				}

				return id;
			}
		}

		/// <summary>Attempts to allocate a slot without throwing when the map is full.</summary>
		/// <param name="id">On failure, set to <c>0</c>.</param>
		public bool TryCreate(TValue val,out int id)
		{
			lock(m_syncRoot)
			{
				return _TryCreate(val,out id);
			}
		}

		/// <summary>Allocates a slot when the caller already holds <see cref="m_syncRoot"/>.</summary>
		private bool _TryCreate(TValue val,out int id)
		{
			if(!_TryAllocate(out var idx,out var gen))
			{
				id = 0;

				return false;
			}

			id = _Commit(idx,gen,val);

			return true;
		}

		/// <summary>Returns the value for a live handle.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the handle is stale or invalid.</exception>
		public TValue Get(int id)
		{
			lock(m_syncRoot)
			{
				if(!_TryGet(id,out var val))
				{
					throw new InvalidOperationException($"Handle {id} is invalid or stale.");
				}

				return val;
			}
		}

		/// <summary>Resolves a handle when it still refers to a live entry.</summary>
		public bool TryGet(int id,out TValue val)
		{
			lock(m_syncRoot)
			{
				return _TryGet(id,out val);
			}
		}

		/// <summary>Resolves a live handle when the caller already holds <see cref="m_syncRoot"/>.</summary>
		private bool _TryGet(int id,out TValue val)
		{
			val = default!;

			if(id == 0)
			{
				return false;
			}

			var idx = _UnpackIndex(id);
			var gen = _UnpackGeneration(id);

			if(idx >= m_slotList.Count)
			{
				return false;
			}

			var slot = m_slotList[idx];

			if(!slot.IsAlive || slot.Generation != gen)
			{
				return false;
			}

			val = slot.Value;

			return true;
		}

		/// <summary>Whether <paramref name="id"/> currently refers to a live entry.</summary>
		public bool IsValid(int id)
		{
			lock(m_syncRoot)
			{
				return _TryGet(id,out _);
			}
		}

		/// <summary>Removes the entry referenced by <paramref name="id"/> when still live.</summary>
		public bool Destroy(int id)
		{
			lock(m_syncRoot)
			{
				if(!_TryGet(id,out _))
				{
					return false;
				}

				var idx = _UnpackIndex(id);

				var slot = m_slotList[idx];
				slot.IsAlive = false;
				slot.Value = default!;

				m_slotList[idx] = slot;

				m_freeIndexStack.Push(idx);
				m_aliveCount--;

				return true;
			}
		}

		/// <summary>Removes every live entry and resets slot reuse state.</summary>
		public void Clear()
		{
			lock(m_syncRoot)
			{
				m_slotList.Clear();
				m_freeIndexStack.Clear();
				m_aliveCount = 0;
			}
		}

		/// <summary>Writes a live slot and returns the packed handle. Caller must hold <see cref="m_syncRoot"/>.</summary>
		private int _Commit(int idx,int gen,TValue val)
		{
			if(gen == 1 && idx == m_slotList.Count)
			{
				m_slotList.Add(new Slot
				{
					Generation = gen,
					IsAlive = true,
					Value = val,
				});
			}
			else
			{
				var slot = m_slotList[idx];
				slot.Generation = gen;
				slot.IsAlive = true;
				slot.Value = val;
				m_slotList[idx] = slot;
			}

			m_aliveCount++;

			return (gen << c_indexBits) | idx;
		}

		/// <summary>Reserves a free or new slot index. Caller must hold <see cref="m_syncRoot"/>.</summary>
		/// <remarks>Slots that reached <see cref="c_maxGeneration"/> are discarded from the free list.</remarks>
		private bool _TryAllocate(out int idx,out int gen)
		{
			while(m_freeIndexStack.Count > 0)
			{
				idx = m_freeIndexStack.Pop();
				var slot = m_slotList[idx];

				if(slot.Generation >= c_maxGeneration)
				{
					continue;
				}

				gen = slot.Generation+1;

				return true;
			}

			if(m_slotList.Count <= c_maxIndex)
			{
				idx = m_slotList.Count;
				gen = 1;

				return true;
			}

			idx = 0;
			gen = 0;

			return false;
		}

		private static int _UnpackIndex(int id)
		{
			return id & c_indexMask;
		}

		private static int _UnpackGeneration(int id)
		{
			return (int)((uint)id >> c_indexBits);
		}
	}
}