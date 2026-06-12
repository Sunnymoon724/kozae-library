using System;
using System.Collections;
using System.Collections.Generic;

namespace KZLib.Collections.Generic
{
	/// <summary>
	/// Thread-safe array-backed binary heap with configurable min/max ordering.
	/// </summary>
	/// <typeparam name="TValue">Element type that supports comparison.</typeparam>
	/// <remarks>
	/// Public mutating methods synchronize on an internal lock. Enumeration copies the
	/// backing list before yielding so callers never iterate a live, mutating buffer.
	/// </remarks>
	public abstract class BinaryHeap<TValue> : IEnumerable<TValue>,IEnumerable,IReadOnlyCollection<TValue>,ICollection where TValue : IComparable<TValue>
	{
		private readonly List<TValue> m_valueList;
		private readonly object m_syncRoot = new();

		/// <summary>
		/// Compares two elements using heap ordering (negative when <paramref name="first"/> has higher priority).
		/// </summary>
		protected abstract int Compare(TValue first,TValue second);

		/// <summary>
		/// Creates an empty heap with optional initial list capacity.
		/// </summary>
		/// <param name="capacity">Initial capacity of the internal list.</param>
		public BinaryHeap(int capacity = 0)
		{
			if(capacity < 0)
			{
				throw new ArgumentOutOfRangeException($"The capacity is {capacity}.");
			}

			m_valueList = new List<TValue>(capacity);
		}

		/// <summary>
		/// Builds a heap from an existing collection in O(n) via bottom-up heapify.
		/// </summary>
		/// <param name="collection">Source values; copied into the heap.</param>
		public BinaryHeap(ICollection<TValue> collection)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			m_valueList = new List<TValue>(collection);

			// Last non-leaf index down to root.
			for(var i=m_valueList.Count/2-1;i>=0;i--)
			{
				_HeapifyDown(i);
			}
		}


		/// <summary>
		/// Inserts a value and restores the heap property by sifting up.
		/// </summary>
		public void Insert(TValue value)
		{
			lock(m_syncRoot)
			{
				m_valueList.Add(value);
				_HeapifyUp(m_valueList.Count-1);
			}
		}

		/// <summary>
		/// Removes and returns the top-priority element.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the heap is empty.</exception>
		public TValue ExtractTop()
		{
			lock(m_syncRoot)
			{
				if(m_valueList.Count == 0)
				{
					throw new InvalidOperationException("Heap is empty.");
				}

				var top = m_valueList[0];

				// Move the last leaf to the root, then sift down.
				m_valueList[0] = m_valueList[^1];
				m_valueList.RemoveAt(m_valueList.Count-1);

				if(m_valueList.Count > 0)
				{
					_HeapifyDown(0);
				}

				return top;
			}
		}

		/// <summary>
		/// Returns the top-priority element without removing it.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the heap is empty.</exception>
		public TValue Peek()
		{
			lock(m_syncRoot)
			{
				if(m_valueList.Count == 0)
				{
					throw new InvalidOperationException("Heap is empty.");
				}

				return m_valueList[0];
			}
		}

		/// <summary>
		/// Removes the first equal occurrence of <paramref name="value"/> if present.
		/// </summary>
		/// <returns><see langword="true"/> when a matching element was removed.</returns>
		/// <remarks>Locates the element with a linear scan; both sift directions may run after replacement.</remarks>
		public bool Remove(TValue value)
		{
			lock(m_syncRoot)
			{
				var index = m_valueList.IndexOf(value);

				if(index == -1)
				{
					return false;
				}

				m_valueList[index] = m_valueList[^1];
				m_valueList.RemoveAt(m_valueList.Count-1);

				if(index < m_valueList.Count)
				{
					_HeapifyDown(index);
					_HeapifyUp(index);
				}

				return true;
			}
		}

		/// <summary>Sifts the element at <paramref name="index"/> toward the root while it outranks its parent.</summary>
		private void _HeapifyUp(int index)
		{
			while(index > 0)
			{
				var parent = (index-1)/2;

				if(Compare(m_valueList[index],m_valueList[parent]) >= 0)
				{
					break;
				}

				_Swap(index,parent);

				index = parent;
			}
		}

		/// <summary>Sifts the element at <paramref name="index"/> toward the leaves while a child outranks it.</summary>
		private void _HeapifyDown(int index)
		{
			var last = m_valueList.Count-1;

			while(index <= last)
			{
				var left = index*2+1;
				var right = index*2+2;
				var swap = index;

				if(left <= last && Compare(m_valueList[left],m_valueList[swap]) < 0)
				{
					swap = left;
				}

				if(right <= last && Compare(m_valueList[right],m_valueList[swap]) < 0)
				{
					swap = right;
				}

				if(swap == index)
				{
					break;
				}

				_Swap(index,swap);

				index = swap;
			}
		}

		private void _Swap(int lhs,int rhs)
		{
			(m_valueList[rhs],m_valueList[lhs]) = (m_valueList[lhs],m_valueList[rhs]);
		}

		/// <summary>Returns a snapshot enumeration of all heap elements (order is not priority order).</summary>
		public IEnumerator<TValue> GetEnumerator()
		{
			TValue[] snapshotArray;

			lock(m_syncRoot)
			{
				snapshotArray = m_valueList.ToArray();
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

		/// <summary>Copies heap elements into a strongly typed array segment.</summary>
		public void CopyTo(Array array,int index)
		{
			if(array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if(index < 0 || index > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			lock(m_syncRoot)
			{
				var count = m_valueList.Count;

				if(index+count > array.Length)
				{
					throw new ArgumentException("Destination array is too small.");
				}

				if(count == 0)
				{
					return;
				}

				if(array is TValue[] convert)
				{
					m_valueList.CopyTo(convert,index);
				}
				else
				{
					throw new InvalidCastException("Invalid array type.");
				}
			}
		}

		/// <summary>Performs a linear scan for <paramref name="value"/>.</summary>
		public bool Contains(TValue value)
		{
			lock(m_syncRoot)
			{
				return m_valueList.Contains(value);
			}
		}

		/// <summary>Whether the heap contains no elements.</summary>
		public bool IsEmpty
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_valueList.Count == 0;
				}
			}
		}

		/// <summary>Number of elements currently stored.</summary>
		public int Count
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_valueList.Count;
				}
			}
		}

		int ICollection.Count => Count;

		/// <summary>Individual public methods lock internally; external composite operations should use <see cref="SyncRoot"/>.</summary>
		public bool IsSynchronized => true;

		/// <inheritdoc />
		public object SyncRoot => m_syncRoot;
	}

	/// <summary>Binary heap whose root holds the smallest element.</summary>
	public sealed class MinHeap<TValue> : BinaryHeap<TValue> where TValue : IComparable<TValue>
	{
		protected override int Compare(TValue first,TValue second)
		{
			return first.CompareTo(second);
		}
	}

	/// <summary>Binary heap whose root holds the largest element.</summary>
	public sealed class MaxHeap<TValue> : BinaryHeap<TValue> where TValue : IComparable<TValue>
	{
		protected override int Compare(TValue first,TValue second)
		{
			return second.CompareTo(first);
		}
	}
}