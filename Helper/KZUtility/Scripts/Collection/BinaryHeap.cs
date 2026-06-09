using System;
using System.Collections;
using System.Collections.Generic;

namespace KZLib.Collections.Generic
{
	public abstract class BinaryHeap<TValue> : IEnumerable<TValue>,IEnumerable,IReadOnlyCollection<TValue>,ICollection where TValue : IComparable<TValue>
	{
		private readonly List<TValue> m_valueList;
		private readonly object m_syncRoot = new();

		protected abstract int Compare(TValue first,TValue second);

		public BinaryHeap(int capacity = 0)
		{
			if(capacity < 0)
			{
				throw new ArgumentOutOfRangeException($"The capacity is {capacity}.");
			}

			m_valueList = new List<TValue>(capacity);
		}

		public BinaryHeap(ICollection<TValue> collection)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			m_valueList = new List<TValue>(collection);

			for(var i=m_valueList.Count/2-1;i>=0;i--)
			{
				_HeapifyDown(i);
			}
		}


		public void Insert(TValue value)
		{
			lock(m_syncRoot)
			{
				m_valueList.Add(value);
				_HeapifyUp(m_valueList.Count-1);
			}
		}

		public TValue ExtractTop()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					throw new InvalidOperationException("Heap is empty.");
				}

				var top = m_valueList[0];

				m_valueList[0] = m_valueList[^1];
				m_valueList.RemoveAt(m_valueList.Count-1);

				if(!IsEmpty)
				{
					_HeapifyDown(0);
				}

				return top;
			}
		}

		public TValue Peek()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					throw new InvalidOperationException("Heap is empty.");
				}

				return m_valueList[0];
			}
		}

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

		public bool Contains(TValue value)
		{
			lock(m_syncRoot)
			{
				return m_valueList.Contains(value);
			}
		}

		public bool IsEmpty => Count == 0;

		public int Count => m_valueList.Count;

		public bool IsSynchronized => false;
		public object SyncRoot => m_syncRoot;
	}

	public sealed class MinHeap<TValue> : BinaryHeap<TValue> where TValue : IComparable<TValue>
	{
		protected override int Compare(TValue first,TValue second)
		{
			return first.CompareTo(second);
		}
	}

	public sealed class MaxHeap<TValue> : BinaryHeap<TValue> where TValue : IComparable<TValue>
	{
		protected override int Compare(TValue first,TValue second)
		{
			return second.CompareTo(first);
		}
	}
}