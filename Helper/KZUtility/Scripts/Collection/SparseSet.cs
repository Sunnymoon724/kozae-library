using System;
using System.Collections;
using System.Collections.Generic;

namespace KZLib.Collections.Generic
{
	/// <summary>
	/// Thread-safe sparse set for tracking live integer indices in <c>[0, <see cref="Capacity"/>)</c>.
	/// </summary>
	/// <remarks>
	/// Supports O(1) add, remove, and membership checks, and iterates only live indices in dense order.
	/// Pair with a parallel value array (for example component data keyed by the same index) or with
	/// <see cref="KZLib.Utilities.SlotMap{TValue}"/> handles whose slot index is stored here.
	/// </remarks>
	public sealed class SparseSet : IEnumerable<int>,IEnumerable,IReadOnlyCollection<int>
	{
		private readonly int[] m_denseArray = null!;
		private readonly int[] m_sparseArray = null!;
		private readonly object m_syncRoot = new();

		private int m_count = 0;

		/// <summary>Maximum index plus one; valid indices are <c>0</c> to <see cref="Capacity"/> - 1.</summary>
		public int Capacity => m_denseArray.Length;

		/// <summary>Creates an empty set that can track indices in <c>[0, <paramref name="capacity"/>)</c>.</summary>
		/// <param name="capacity">Upper bound on storable indices; must be positive.</param>
		public SparseSet(int capacity)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}

			m_denseArray = new int[capacity];
			m_sparseArray = new int[capacity];
		}

		/// <summary>Number of live indices currently stored.</summary>
		public int Count
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_count;
				}
			}
		}

		/// <summary>Whether the set holds no indices.</summary>
		public bool IsEmpty
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_count == 0;
				}
			}
		}

		/// <summary>Whether every valid index is currently in the set.</summary>
		public bool IsFull
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_count >= m_denseArray.Length;
				}
			}
		}

		/// <summary>Whether <paramref name="idx"/> is currently in the set.</summary>
		public bool Contains(int idx)
		{
			lock(m_syncRoot)
			{
				return _ContainsIndex(idx);
			}
		}

		/// <summary>Adds <paramref name="idx"/> when it is not already present.</summary>
		/// <returns><see langword="true"/> when the index was added.</returns>
		public bool TryAdd(int idx)
		{
			lock(m_syncRoot)
			{
				return _TryAdd(idx);
			}
		}

		/// <summary>Adds <paramref name="idx"/>.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the index is already present.</exception>
		public void Add(int idx)
		{
			lock(m_syncRoot)
			{
				if(!_TryAdd(idx))
				{
					throw new InvalidOperationException($"Index {idx} is already in the set.");
				}
			}
		}

		private bool _TryAdd(int idx)
		{
			if(!_IsValidIndex(idx,m_denseArray.Length) || m_count >= m_denseArray.Length)
			{
				return false;
			}

			if(_ContainsIndex(idx))
			{
				return false;
			}

			m_denseArray[m_count] = idx;
			m_sparseArray[idx] = m_count;
			m_count++;

			return true;
		}

		/// <summary>Removes <paramref name="idx"/> when it is present.</summary>
		/// <returns><see langword="true"/> when the index was removed.</returns>
		public bool TryRemove(int idx)
		{
			lock(m_syncRoot)
			{
				return _TryRemove(idx);
			}
		}

		/// <summary>Removes <paramref name="idx"/>.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the index is not present.</exception>
		public void Remove(int idx)
		{
			lock(m_syncRoot)
			{
				if(!_TryRemove(idx))
				{
					throw new InvalidOperationException($"Index {idx} is not in the set.");
				}
			}
		}

		private bool _TryRemove(int idx)
		{
			if(!_IsValidIndex(idx,m_denseArray.Length))
			{
				return false;
			}

			var denseIndex = m_sparseArray[idx];

			if(!_IsValidIndex(denseIndex,m_count) || m_denseArray[denseIndex] != idx)
			{
				return false;
			}

			var lastIndex = m_denseArray[m_count-1];

			m_denseArray[denseIndex] = lastIndex;
			m_sparseArray[lastIndex] = denseIndex;
			m_count--;

			return true;
		}

		/// <summary>Removes every index from the set.</summary>
		public void Clear()
		{
			lock(m_syncRoot)
			{
				m_count = 0;
			}
		}

		/// <summary>Materializes the live indices in dense iteration order.</summary>
		public int[] ToArray()
		{
			lock(m_syncRoot)
			{
				if(m_count == 0)
				{
					return Array.Empty<int>();
				}

				var retArray = new int[m_count];

				Array.Copy(m_denseArray,retArray,m_count);

				return retArray;
			}
		}

		/// <summary>Returns live indices in dense order as a snapshot.</summary>
		public IEnumerator<int> GetEnumerator()
		{
			int[] snapshotArray;

			lock(m_syncRoot)
			{
				snapshotArray = new int[m_count];
				Array.Copy(m_denseArray,snapshotArray,m_count);
			}

			for(var i=0;i<snapshotArray.Length;i++)
			{
				yield return snapshotArray[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private bool _ContainsIndex(int idx)
		{
			if(!_IsValidIndex(idx,m_denseArray.Length))
			{
				return false;
			}

			var denseIndex = m_sparseArray[idx];

			if(!_IsValidIndex(denseIndex,m_count))
			{
				return false;
			}

			return m_denseArray[denseIndex] == idx;
		}

		private bool _IsValidIndex(int idx,int length)
		{
			return 0 <= idx && idx < length;
		}
	}
}