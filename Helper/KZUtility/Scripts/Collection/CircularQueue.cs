using System;
using System.Collections;
using System.Collections.Generic;

namespace KZLib.Collections.Generic
{
	/// <summary>
	/// Fixed-capacity, thread-safe circular queue backed by a ring buffer.
	/// </summary>
	/// <typeparam name="TValue">Element type stored in the queue.</typeparam>
	/// <remarks>
	/// When full, <see cref="Enqueue"/> overwrites the oldest element by advancing the front index.
	/// Empty state is represented by <c>m_front == -1</c>.
	/// </remarks>
	public sealed class CircularQueue<TValue> : IEnumerable<TValue>,IEnumerable,IReadOnlyCollection<TValue>,ICollection
	{
		private readonly TValue[] m_valueArray = Array.Empty<TValue>();
		private readonly int m_capacity = 0;
		private readonly object m_syncRoot = new();

		/// <summary>Index of the oldest element, or -1 when empty.</summary>
		private int m_front = -1;

		/// <summary>Index of the most recently enqueued element, or -1 when empty.</summary>
		private int m_rear = -1;

		/// <summary>Maximum number of elements the buffer can hold before overwriting.</summary>
		public int Capacity => m_capacity;

		/// <summary>
		/// Creates an empty queue with the given fixed capacity.
		/// </summary>
		/// <param name="capacity">Buffer size; must be positive.</param>
		public CircularQueue(int capacity)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}

			m_capacity = capacity;
			m_valueArray = new TValue[capacity];
		}

		/// <summary>
		/// Creates a queue pre-filled with every element from <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">Non-empty source whose count becomes the queue capacity.</param>
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

		/// <summary>
		/// Appends a value at the rear, dropping the front element when the buffer is full.
		/// </summary>
		public void Enqueue(TValue val)
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
				m_valueArray[m_rear] = val;
			}
		}

		/// <summary>Returns the front element without removing it.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
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

		/// <summary>Removes and returns the front element.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
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

		/// <summary>Number of elements currently stored.</summary>
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

		/// <summary>Whether the queue holds no elements.</summary>
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

		/// <summary>Whether the next enqueue would overwrite the oldest element.</summary>
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

		/// <summary>Removes all elements and resets front/rear indices.</summary>
		public void Clear()
		{
			lock(m_syncRoot)
			{
				m_front = -1;
				m_rear = -1;

				Array.Clear(m_valueArray,0,m_capacity);
			}
		}

		/// <summary>Returns elements from front to rear as a snapshot (FIFO order).</summary>
		public IEnumerator<TValue> GetEnumerator()
		{
			TValue[] snapshotArray;

			lock(m_syncRoot)
			{
				var count = Count;
				snapshotArray = new TValue[count];

				var idx = m_front;

				for(var i=0;i<count;i++)
				{
					snapshotArray[i] = m_valueArray[idx];
					idx = (idx+1)%m_capacity;
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

		/// <summary>Scans the logical queue order for an equal element.</summary>
		public bool Contains(TValue val)
		{
			lock(m_syncRoot)
			{
				var idx = m_front;
				var comparer = EqualityComparer<TValue>.Default;
				var count = Count;

				for(var i=0;i<count;i++)
				{
					if(comparer.Equals(m_valueArray[idx],val))
					{
						return true;
					}

					idx = (idx+1)%m_capacity;
				}
			}

			return false;
		}

		/// <summary>Copies elements in FIFO order into <paramref name="array"/> starting at <paramref name="idx"/>.</summary>
		public void CopyTo(Array array,int idx)
		{
			if(array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if(idx < 0 || idx > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(idx));
			}

			lock(m_syncRoot)
			{
				var count = Count;

				if(idx+count > array.Length)
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
					array.SetValue(m_valueArray[front],idx+i);
					front = (front+1)%m_capacity;
				}
			}
		}

		/// <inheritdoc />
		public bool IsSynchronized => false;

		/// <inheritdoc />
		public object SyncRoot => m_syncRoot;

		/// <summary>Materializes the current queue contents into a new array in FIFO order.</summary>
		public TValue[] ToArray()
		{
			lock(m_syncRoot)
			{
				var count = Count;

				if(count == 0)
				{
					return Array.Empty<TValue>();
				}

				var retArray = new TValue[count];
				var idx = m_front;

				for(var i=0;i<count;i++)
				{
					retArray[i] = m_valueArray[idx];
					idx = (idx+1)%m_capacity;
				}

				return retArray;
			}
		}
	}
}