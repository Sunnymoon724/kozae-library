using System;
using System.Collections;
using System.Collections.Generic;

namespace KZLib.Collections.Generic
{
	public sealed class CircularQueue<TValue> : IEnumerable<TValue>,IEnumerable,IReadOnlyCollection<TValue>,ICollection
	{
		private readonly TValue[] m_valueArray = Array.Empty<TValue>();
		private readonly int m_capacity = 0;
		private readonly object m_syncRoot = new();

		private int m_front = -1;
		private int m_rear = -1;

		public int Capacity => m_capacity;

		public CircularQueue(int capacity)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}

			m_capacity = capacity;
			m_valueArray = new TValue[capacity];
		}

		public CircularQueue(ICollection<TValue> collection)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if(collection.Count == 0)
			{
				throw new ArgumentException("Collection must not be empty.", nameof(collection));
			}

			m_capacity = collection.Count;
			m_valueArray = new TValue[m_capacity];

			collection.CopyTo(m_valueArray,0);

			m_front = 0;
			m_rear = m_capacity-1;
		}

		public void Enqueue(TValue value)
		{
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
					throw new InvalidOperationException("Queue is empty.");
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
					throw new InvalidOperationException("Queue is empty.");
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

		int ICollection.Count => Count;

		public int Count
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_front == -1 ? 0 : (m_rear >= m_front ? m_rear-m_front+1 : m_capacity-m_front+m_rear+1);
				}
			}
		}

		public bool IsEmpty
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_front == -1;
				}
			}
		}

		public bool IsFull
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_front != -1 && (m_rear+1)%m_capacity == m_front;
				}
			}
		}

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
			TValue[] snapshotArray;

			lock(m_syncRoot)
			{
				var count = Count;
				snapshotArray = new TValue[count];

				var index = m_front;

				for(var i=0;i<count;i++)
				{
					snapshotArray[i] = m_valueArray[index];
					index = (index+1)%m_capacity;
				}
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

		public bool Contains(TValue value)
		{
			lock(m_syncRoot)
			{
				var index = m_front;
				var comparer = EqualityComparer<TValue>.Default;
				var count = Count;

				for(var i=0;i<count;i++)
				{
					if(comparer.Equals(m_valueArray[index],value))
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
				throw new ArgumentNullException(nameof(array));
			}

			if(index < 0 || index > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			lock(m_syncRoot)
			{
				var count = Count;

				if(index+count > array.Length)
				{
					throw new ArgumentException("Destination array is too small.");
				}

				if(count == 0)
				{
					return;
				}

				var front = m_front;

				for(var i=0;i<count;i++)
				{
					array.SetValue(m_valueArray[front],index+i);
					front = (front+1)%m_capacity;
				}
			}
		}

		public bool IsSynchronized => false;
		public object SyncRoot => m_syncRoot;

		public TValue[] ToArray()
		{
			lock(m_syncRoot)
			{
				var count = Count;

				if(count == 0)
				{
					return Array.Empty<TValue>();
				}

				var resultArray = new TValue[count];
				var index = m_front;

				for(var i=0;i<count;i++)
				{
					resultArray[i] = m_valueArray[index];
					index = (index+1)%m_capacity;
				}

				return resultArray;
			}
		}
	}
}