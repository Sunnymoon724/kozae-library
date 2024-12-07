
namespace System.Collections.Generic
{
	public class CircularQueue<TData> : IEnumerable<TData>,IEnumerable,IReadOnlyCollection<TData>,ICollection
	{
		private readonly TData[] _dataArray = Array.Empty<TData>();
		private readonly int _capacity = 0;
		private readonly object _syncRoot = new object();

		private int m_Front = -1;
		private int m_Rear = -1;

		public int Size => _dataArray.Length;
		public int Capacity => _capacity;

		public CircularQueue(int capacity)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException($"The capacity is {capacity}.");
			}

			_capacity = capacity;
			_dataArray = new TData[capacity];
		}

		public CircularQueue(ICollection<TData> collection) : this(collection.Count)
		{
			collection.CopyTo(_dataArray,0);

			m_Rear = _capacity-1;
		}

		public void Enqueue(TData data)
		{
			if(data == null)
			{
				throw new ArgumentNullException("Data cannot be null.");
			}

			lock(_syncRoot)
			{
				if(IsFull)
				{
					m_Front = (m_Front+1)%_capacity;
				}
				else if(IsEmpty)
				{
					m_Front = 0;
				}

				m_Rear = (m_Rear+1)%_capacity;
				_dataArray[m_Rear] = data;
			}
		}

		public TData Peek()
		{
			lock(_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Queue is empty.");
				}

				return _dataArray[m_Front];
			}
		}

		public TData Dequeue()
		{
			lock(_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Queue is empty.");
				}

				var data = _dataArray[m_Front];

				if(m_Front == m_Rear)
				{
					m_Front = -1;
					m_Rear = -1;
				}
				else
				{
					m_Front = (m_Front+1)%_capacity;
				}

				return data;
			}
		}

		public int Count
		{
			get
			{
				lock(_syncRoot)
				{
					return IsEmpty ? 0 : (m_Rear >= m_Front ? m_Rear-m_Front+1 : _capacity-m_Front+m_Rear+1);
				}
			}
		}

		public bool IsEmpty => m_Front == -1;
		public bool IsFull => (m_Rear+1)%_capacity == m_Front;

		public void Clear()
		{
			lock(_syncRoot)
			{
				m_Front = -1;
				m_Rear = -1;

				Array.Clear(_dataArray,0,_capacity);
			}
		}

		public IEnumerator<TData> GetEnumerator()
		{
			lock(_syncRoot)
			{
				if(IsEmpty)
				{
					yield break;
				}

				var index = m_Front;

				while(index != m_Rear)
				{
					yield return _dataArray[index];

					index = (index+1)%_capacity;
				}

				yield return _dataArray[index];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Contains(TData data)
		{
			if(data == null)
			{
				throw new ArgumentNullException("Data cannot be null.");
			}

			lock(_syncRoot)
			{
				var index = m_Front;

				for(var i=0;i<Count;i++)
				{
					if(EqualityComparer<TData>.Default.Equals(_dataArray[index],data))
					{
						return true;
					}

					index = (index+1)%_capacity;
				}
			}

			return false;
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

			if(Count > 0)
			{
				lock(_syncRoot)
				{
					var front = m_Front;

					for(var i=0;i<Count;i++)
					{
						array.SetValue(_dataArray[front],index+i);
						front = (front+1)%_capacity;
					}
				}
			}
		}

		public bool IsSynchronized => true;
		public object SyncRoot => _syncRoot;
	}
}