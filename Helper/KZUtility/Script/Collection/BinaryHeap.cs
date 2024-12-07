
namespace System.Collections.Generic
{
	public abstract class BinaryHeap<TData> : IEnumerable<TData>,IEnumerable,IReadOnlyCollection<TData>,ICollection where TData : IComparable<TData>
	{
		private readonly List<TData> _dataList = new List<TData>();
		private readonly object _syncRoot = new object();

		protected abstract int Compare(TData first,TData second);

		public BinaryHeap(int capacity = 0)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException($"The capacity is {capacity}.");
			}

			_dataList = new List<TData>(capacity);
		}

		public BinaryHeap(ICollection<TData> collection) : this(collection.Count)
		{
			for(var i=_dataList.Count/2-1;i>=0;i--)
			{
				HeapifyDown(i);
			}
		}


		public void Insert(TData data)
		{
			lock(_syncRoot)
			{
				_dataList.Add(data);
				HeapifyUp(_dataList.Count-1);
			}
		}

		public TData ExtractTop()
		{
			lock(_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Heap is empty.");
				}

				var top = _dataList[0];

				_dataList[0] = _dataList[^1];
				_dataList.RemoveAt(_dataList.Count-1);

				if(!IsEmpty)
				{
					HeapifyDown(0);
				}

				return top;
			}
		}

		public TData Peek()
		{
			lock(_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Heap is empty.");
				}

				return _dataList[0];
			}
		}

		public bool Remove(TData data)
		{
			lock(_syncRoot)
			{
				var index = _dataList.IndexOf(data);

				if(index == -1)
				{
					return false;
				}

				_dataList[index] = _dataList[^1];
				_dataList.RemoveAt(_dataList.Count-1);

				if(index < _dataList.Count)
				{
					HeapifyDown(index);
					HeapifyUp(index);
				}

				return true;
			}
		}

		private void HeapifyUp(int index)
		{
			while(index > 0)
			{
				var parent = (index-1)/2;

				if(Compare(_dataList[index],_dataList[parent]) >= 0)
				{
					break;
				}

				Swap(index,parent);

				index = parent;
			}
		}

		private void HeapifyDown(int index)
		{
			var last = _dataList.Count-1;

			while(index <= last)
			{
				var left = index*2+1;
				var right = index*2+2;
				var swap = index;

				if(left <= last && Compare(_dataList[left],_dataList[swap]) < 0)
				{
					swap = left;
				}

				if(right <= last && Compare(_dataList[right],_dataList[swap]) < 0)
				{
					swap = right;
				}

				if(swap == index)
				{
					break;
				}

				Swap(index,swap);

				index = swap;
			}
		}

		private void Swap(int prev,int next)
		{
			(_dataList[next],_dataList[prev]) = (_dataList[prev],_dataList[next]);
		}

		public IEnumerator<TData> GetEnumerator()
		{
			lock(_syncRoot)
			{
				return _dataList.GetEnumerator();
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
				throw new ArgumentNullException("Array cannot be null.");
			}

			if(index < 0 || index >= array.Length)
			{
				throw new ArgumentOutOfRangeException($"Index {index} is out of bounds for the array.");
			}

			lock(_syncRoot)
			{
				if(array is TData[] convert)
				{
					_dataList.CopyTo(convert,index);
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
				throw new ArgumentNullException("Data cannot be null.");
			}

			lock(_syncRoot)
			{
				return _dataList.Contains(data);
			}
		}

		public bool IsEmpty => Count == 0;

		public int Count => _dataList.Count;

		public bool IsSynchronized => true;
		public object SyncRoot => _syncRoot;
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