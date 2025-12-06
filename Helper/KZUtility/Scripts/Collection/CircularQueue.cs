
namespace System.Collections.Generic
{
	public class CircularQueue<TData> : IEnumerable<TData>,IEnumerable,IReadOnlyCollection<TData>,ICollection
	{
		private readonly TData[] m_dataArray = Array.Empty<TData>();
		private readonly int m_capacity = 0;
		private readonly object m_syncRoot = new();

		private int m_front = -1;
		private int m_rear = -1;

		public int Size => m_dataArray.Length;
		public int Capacity => m_capacity;

		public CircularQueue(int capacity)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException($"The capacity is {capacity}.");
			}

			m_capacity = capacity;
			m_dataArray = new TData[capacity];
		}

		public CircularQueue(ICollection<TData> collection) : this(collection.Count)
		{
			collection.CopyTo(m_dataArray,0);

			m_rear = m_capacity-1;
		}

		public void Enqueue(TData data)
		{
			if(data == null)
			{
				throw new NullReferenceException("Data cannot be null.");
			}

			lock(m_syncRoot)
			{
				if(IsFull)
				{
					m_front = (m_front+1)%m_capacity;
				}
				else if(IsEmpty)
				{
					m_front = 0;
				}

				m_rear = (m_rear+1)%m_capacity;
				m_dataArray[m_rear] = data;
			}
		}

		public TData Peek()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Queue is empty.");
				}

				return m_dataArray[m_front];
			}
		}

		public TData Dequeue()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Queue is empty.");
				}

				var data = m_dataArray[m_front];

				if(m_front == m_rear)
				{
					m_front = -1;
					m_rear = -1;
				}
				else
				{
					m_front = (m_front+1)%m_capacity;
				}

				return data;
			}
		}

		public int Count
		{
			get
			{
				lock(m_syncRoot)
				{
					return IsEmpty ? 0 : (m_rear >= m_front ? m_rear-m_front+1 : m_capacity-m_front+m_rear+1);
				}
			}
		}

		public bool IsEmpty => m_front == -1;
		public bool IsFull => (m_rear+1)%m_capacity == m_front;

		public void Clear()
		{
			lock(m_syncRoot)
			{
				m_front = -1;
				m_rear = -1;

				Array.Clear(m_dataArray,0,m_capacity);
			}
		}

		public IEnumerator<TData> GetEnumerator()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					yield break;
				}

				var index = m_front;

				while(index != m_rear)
				{
					yield return m_dataArray[index];

					index = (index+1)%m_capacity;
				}

				yield return m_dataArray[index];
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
				throw new NullReferenceException("Data cannot be null.");
			}

			lock(m_syncRoot)
			{
				var index = m_front;

				for(var i=0;i<Count;i++)
				{
					if(EqualityComparer<TData>.Default.Equals(m_dataArray[index],data))
					{
						return true;
					}

					index = (index+1)%m_capacity;
				}
			}

			return false;
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

			if(Count > 0)
			{
				lock(m_syncRoot)
				{
					var front = m_front;

					for(var i=0;i<Count;i++)
					{
						array.SetValue(m_dataArray[front],index+i);
						front = (front+1)%m_capacity;
					}
				}
			}
		}

		public bool IsSynchronized => true;
		public object SyncRoot => m_syncRoot;
	}
}