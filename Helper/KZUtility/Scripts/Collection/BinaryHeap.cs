
namespace System.Collections.Generic
{
	public abstract class BinaryHeap<TData> : IEnumerable<TData>,IEnumerable,IReadOnlyCollection<TData>,ICollection where TData : IComparable<TData>
	{
		private readonly List<TData> m_dataList = new();
		private readonly object m_syncRoot = new();

		protected abstract int Compare(TData first,TData second);

		public BinaryHeap(int capacity = 0)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException($"The capacity is {capacity}.");
			}

			m_dataList = new List<TData>(capacity);
		}

		public BinaryHeap(ICollection<TData> collection) : this(collection.Count)
		{
			for(var i=m_dataList.Count/2-1;i>=0;i--)
			{
				_HeapifyDown(i);
			}
		}


		public void Insert(TData data)
		{
			lock(m_syncRoot)
			{
				m_dataList.Add(data);
				_HeapifyUp(m_dataList.Count-1);
			}
		}

		public TData ExtractTop()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Heap is empty.");
				}

				var top = m_dataList[0];

				m_dataList[0] = m_dataList[^1];
				m_dataList.RemoveAt(m_dataList.Count-1);

				if(!IsEmpty)
				{
					_HeapifyDown(0);
				}

				return top;
			}
		}

		public TData Peek()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Heap is empty.");
				}

				return m_dataList[0];
			}
		}

		public bool Remove(TData data)
		{
			lock(m_syncRoot)
			{
				var index = m_dataList.IndexOf(data);

				if(index == -1)
				{
					return false;
				}

				m_dataList[index] = m_dataList[^1];
				m_dataList.RemoveAt(m_dataList.Count-1);

				if(index < m_dataList.Count)
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

				if(Compare(m_dataList[index],m_dataList[parent]) >= 0)
				{
					break;
				}

				_Swap(index,parent);

				index = parent;
			}
		}

		private void _HeapifyDown(int index)
		{
			var last = m_dataList.Count-1;

			while(index <= last)
			{
				var left = index*2+1;
				var right = index*2+2;
				var swap = index;

				if(left <= last && Compare(m_dataList[left],m_dataList[swap]) < 0)
				{
					swap = left;
				}

				if(right <= last && Compare(m_dataList[right],m_dataList[swap]) < 0)
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

		private void _Swap(int prev,int next)
		{
			(m_dataList[next],m_dataList[prev]) = (m_dataList[prev],m_dataList[next]);
		}

		public IEnumerator<TData> GetEnumerator()
		{
			lock(m_syncRoot)
			{
				return m_dataList.GetEnumerator();
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
				throw new NullReferenceException("Array cannot be null.");
			}

			if(index < 0 || index >= array.Length)
			{
				throw new ArgumentOutOfRangeException($"Index {index} is out of bounds for the array.");
			}

			lock(m_syncRoot)
			{
				if(array is TData[] convert)
				{
					m_dataList.CopyTo(convert,index);
				}
				else
				{
					throw new InvalidCastException("Invalid array type.");
				}
			}
		}

		public bool Contains(TData data)
		{
			if(data == null)
			{
				throw new NullReferenceException("Data cannot be null.");
			}

			lock(m_syncRoot)
			{
				return m_dataList.Contains(data);
			}
		}

		public bool IsEmpty => Count == 0;

		public int Count => m_dataList.Count;

		public bool IsSynchronized => true;
		public object SyncRoot => m_syncRoot;
	}

	public class MinHeap<TData> : BinaryHeap<TData> where TData : IComparable<TData>
	{
		protected override int Compare(TData first,TData second)
		{
			return first.CompareTo(second);
		}
	}

	public class MaxHeap<TData> : BinaryHeap<TData> where TData : IComparable<TData>
	{
		protected override int Compare(TData first,TData second)
		{
			return second.CompareTo(first);
		}
	}
}