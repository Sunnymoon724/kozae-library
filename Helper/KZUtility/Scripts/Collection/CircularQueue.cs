
namespace System.Collections.Generic
{
	public sealed class CircularQueue<TValue> : IEnumerable<TValue>,IEnumerable,IReadOnlyCollection<TValue>,ICollection
	{
		private readonly TValue[] m_valueArray = Array.Empty<TValue>();
		private readonly int m_capacity = 0;
		private readonly object m_syncRoot = new();

		private int m_front = -1;
		private int m_rear = -1;

		public int Size => m_valueArray.Length;
		public int Capacity => m_capacity;

		public CircularQueue(int capacity)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException($"The capacity is {capacity}.");
			}

			m_capacity = capacity;
			m_valueArray = new TValue[capacity];
		}

		public CircularQueue(ICollection<TValue> collection) : this(collection.Count)
		{
			collection.CopyTo(m_valueArray,0);

			m_rear = m_capacity-1;
		}

		public void Enqueue(TValue value)
		{
			if(value == null)
			{
				throw new NullReferenceException("Value cannot be null.");
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
				m_valueArray[m_rear] = value;
			}
		}

		public TValue Peek()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Queue is empty.");
				}

				return m_valueArray[m_front];
			}
		}

		public TValue Dequeue()
		{
			lock(m_syncRoot)
			{
				if(IsEmpty)
				{
					throw new ArgumentOutOfRangeException("Queue is empty.");
				}

				var value = m_valueArray[m_front];

				if(m_front == m_rear)
				{
					m_front = -1;
					m_rear = -1;
				}
				else
				{
					m_front = (m_front+1)%m_capacity;
				}

				return value;
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

				Array.Clear(m_valueArray,0,m_capacity);
			}
		}

		public IEnumerator<TValue> GetEnumerator()
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
					yield return m_valueArray[index];

					index = (index+1)%m_capacity;
				}

				yield return m_valueArray[index];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Contains(TValue value)
		{
			if(value == null)
			{
				throw new NullReferenceException("Value cannot be null.");
			}

			lock(m_syncRoot)
			{
				var index = m_front;

				for(var i=0;i<Count;i++)
				{
					if(EqualityComparer<TValue>.Default.Equals(m_valueArray[index],value))
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
						array.SetValue(m_valueArray[front],index+i);
						front = (front+1)%m_capacity;
					}
				}
			}
		}

		public bool IsSynchronized => true;
		public object SyncRoot => m_syncRoot;
	}
}