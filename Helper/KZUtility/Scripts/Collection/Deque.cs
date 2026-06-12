using System;
using System.Collections;
using System.Collections.Generic;

namespace KZLib.Collections.Generic
{
	/// <summary>
	/// Thread-safe double-ended queue backed by a growable ring buffer.
	/// </summary>
	/// <typeparam name="TValue">Element type stored in the deque.</typeparam>
	/// <remarks>
	/// Unlike <see cref="CircularQueue{TValue}"/>, a full deque grows its buffer instead of overwriting elements.
	/// Growth follows the same doubling strategy as <see cref="List{T}"/>; capacity is retained after <see cref="Clear"/>.
	/// Standard queue operations (<see cref="Enqueue"/>, <see cref="Dequeue"/>, <see cref="Peek"/>) use the back for insert and the front for remove.
	/// </remarks>
	public sealed class Deque<TValue> : IEnumerable<TValue>,IEnumerable,IReadOnlyCollection<TValue>,ICollection
	{
		private const int c_defaultCapacity = 16;

		private TValue[] m_valueArray = Array.Empty<TValue>();
		private readonly object m_syncRoot = new();

		/// <summary>Index of the front element.</summary>
		private int m_head = 0;

		private int m_count = 0;

		/// <summary>Creates an empty deque with the default initial capacity.</summary>
		public Deque()
		{
		}

		/// <summary>Creates an empty deque with the given initial capacity.</summary>
		/// <param name="capacity">Initial buffer size; must be zero or greater.</param>
		public Deque(int capacity)
		{
			if(capacity < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}

			if(capacity > 0)
			{
				m_valueArray = new TValue[capacity];
			}
		}

		/// <summary>Creates a deque containing every element from <paramref name="collection"/> in order.</summary>
		/// <param name="collection">Source elements copied from front to back.</param>
		public Deque(ICollection<TValue> collection)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			m_count = collection.Count;

			if(m_count == 0)
			{
				return;
			}

			m_valueArray = new TValue[m_count];
			collection.CopyTo(m_valueArray,0);
		}

		int ICollection.Count => Count;

		/// <summary>Number of elements currently stored.</summary>
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

		/// <summary>Whether the deque holds no elements.</summary>
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

		/// <summary>Current backing buffer length.</summary>
		public int Capacity
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_valueArray.Length;
				}
			}
			set
			{
				if(value < Count)
				{
					throw new ArgumentOutOfRangeException(nameof(value),value,"Capacity cannot be less than Count.");
				}

				lock(m_syncRoot)
				{
					_SetCapacity(value);
				}
			}
		}

		/// <summary>Individual public methods lock internally; external composite operations should use <see cref="SyncRoot"/>.</summary>
		public bool IsSynchronized => false;

		/// <inheritdoc />
		public object SyncRoot => m_syncRoot;

		/// <summary>Appends a value at the back, growing the buffer when full.</summary>
		public void Enqueue(TValue val)
		{
			PushBack(val);
		}

		/// <summary>Removes and returns the front element.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
		public TValue Dequeue()
		{
			return PopFront();
		}

		/// <summary>Returns the front element without removing it.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
		public TValue Peek()
		{
			return PeekFront();
		}

		/// <summary>Attempts to remove the front element.</summary>
		public bool TryDequeue(out TValue val)
		{
			return TryPopFront(out val);
		}

		/// <summary>Inserts a value at the front.</summary>
		public void PushFront(TValue val)
		{
			lock(m_syncRoot)
			{
				_EnsureCapacity(m_count+1);

				m_head = m_head == 0 ? m_valueArray.Length-1 : m_head-1;
				m_valueArray[m_head] = val;
				m_count++;
			}
		}

		/// <summary>Inserts a value at the back.</summary>
		public void PushBack(TValue val)
		{
			lock(m_syncRoot)
			{
				_EnsureCapacity(m_count+1);

				var tail = (m_head+m_count)%m_valueArray.Length;
				m_valueArray[tail] = val;
				m_count++;
			}
		}

		/// <summary>Removes and returns the front element.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
		public TValue PopFront()
		{
			lock(m_syncRoot)
			{
				if(!_TryPopFront(out var val))
				{
					throw new InvalidOperationException("Deque is empty.");
				}

				return val;
			}
		}

		/// <summary>Removes and returns the back element.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
		public TValue PopBack()
		{
			lock(m_syncRoot)
			{
				if(!_TryPopBack(out var val))
				{
					throw new InvalidOperationException("Deque is empty.");
				}

				return val;
			}
		}

		/// <summary>Attempts to remove the front element.</summary>
		public bool TryPopFront(out TValue val)
		{
			lock(m_syncRoot)
			{
				return _TryPopFront(out val);
			}
		}

		/// <summary>Attempts to remove the back element.</summary>
		public bool TryPopBack(out TValue val)
		{
			lock(m_syncRoot)
			{
				return _TryPopBack(out val);
			}
		}

		/// <summary>Returns the front element without removing it.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
		public TValue PeekFront()
		{
			lock(m_syncRoot)
			{
				if(m_count == 0)
				{
					throw new InvalidOperationException("Deque is empty.");
				}

				return m_valueArray[m_head];
			}
		}

		/// <summary>Returns the back element without removing it.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
		public TValue PeekBack()
		{
			lock(m_syncRoot)
			{
				if(m_count == 0)
				{
					throw new InvalidOperationException("Deque is empty.");
				}

				var tail = (m_head+m_count-1)%m_valueArray.Length;

				return m_valueArray[tail];
			}
		}

		/// <summary>Ensures the backing buffer can hold at least <paramref name="capacity"/> elements.</summary>
		/// <param name="capacity">Minimum buffer length; must be zero or greater.</param>
		public void EnsureCapacity(int capacity)
		{
			if(capacity < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}

			lock(m_syncRoot)
			{
				_EnsureCapacity(capacity);
			}
		}

		/// <summary>Removes all elements while retaining the current buffer capacity.</summary>
		public void Clear()
		{
			lock(m_syncRoot)
			{
				if(m_count > 0)
				{
					Array.Clear(m_valueArray,0,m_valueArray.Length);
				}

				m_head = 0;
				m_count = 0;
			}
		}

		/// <summary>Sets the capacity to the current element count when it is below ninety percent of the buffer length.</summary>
		public void TrimExcess()
		{
			lock(m_syncRoot)
			{
				if(m_count == 0)
				{
					m_valueArray = Array.Empty<TValue>();
					m_head = 0;

					return;
				}

				if(m_count < m_valueArray.Length)
				{
					_SetCapacity(m_count);
				}
			}
		}

		/// <summary>Scans the logical deque order for an equal element.</summary>
		public bool Contains(TValue val)
		{
			lock(m_syncRoot)
			{
				var index = m_head;
				var comparer = EqualityComparer<TValue>.Default;

				for(var i=0;i<m_count;i++)
				{
					if(comparer.Equals(m_valueArray[index],val))
					{
						return true;
					}

					index = (index+1)%m_valueArray.Length;
				}
			}

			return false;
		}

		/// <summary>Copies elements in front-to-back order into <paramref name="array"/> starting at <paramref name="idx"/>.</summary>
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
				if(idx+m_count > array.Length)
				{
					throw new ArgumentException("Destination array is too small.");
				}

				if(m_count == 0)
				{
					return;
				}

				var index = m_head;

				for(var i=0;i<m_count;i++)
				{
					array.SetValue(m_valueArray[index],idx+i);
					index = (index+1)%m_valueArray.Length;
				}
			}
		}

		/// <summary>Materializes the current deque contents into a new array in front-to-back order.</summary>
		public TValue[] ToArray()
		{
			lock(m_syncRoot)
			{
				if(m_count == 0)
				{
					return Array.Empty<TValue>();
				}

				var retArray = new TValue[m_count];
				var index = m_head;

				for(var i=0;i<m_count;i++)
				{
					retArray[i] = m_valueArray[index];
					index = (index+1)%m_valueArray.Length;
				}

				return retArray;
			}
		}

		/// <summary>Returns elements from front to back as a snapshot.</summary>
		public IEnumerator<TValue> GetEnumerator()
		{
			TValue[] snapshotArray;

			lock(m_syncRoot)
			{
				snapshotArray = new TValue[m_count];

				var index = m_head;

				for(var i=0;i<m_count;i++)
				{
					snapshotArray[i] = m_valueArray[index];
					index = (index+1)%m_valueArray.Length;
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

		private bool _TryPopFront(out TValue val)
		{
			if(m_count == 0)
			{
				val = default!;

				return false;
			}

			val = m_valueArray[m_head];
			m_valueArray[m_head] = default!;
			m_head = (m_head+1)%m_valueArray.Length;
			m_count--;

			return true;
		}

		private bool _TryPopBack(out TValue val)
		{
			if(m_count == 0)
			{
				val = default!;

				return false;
			}

			var tail = (m_head+m_count-1)%m_valueArray.Length;

			val = m_valueArray[tail];
			m_valueArray[tail] = default!;
			m_count--;

			return true;
		}

		private void _EnsureCapacity(int required)
		{
			if(m_valueArray.Length >= required)
			{
				return;
			}

			var newCapacity = m_valueArray.Length == 0 ? c_defaultCapacity : m_valueArray.Length*2;

			while(newCapacity < required)
			{
				newCapacity *= 2;
			}

			_SetCapacity(newCapacity);
		}

		private void _SetCapacity(int newCapacity)
		{
			if(newCapacity == m_valueArray.Length)
			{
				return;
			}

			if(newCapacity == 0)
			{
				m_valueArray = Array.Empty<TValue>();
				m_head = 0;

				return;
			}

			var newArray = new TValue[newCapacity];

			for(var i=0;i<m_count;i++)
			{
				newArray[i] = m_valueArray[(m_head+i)%m_valueArray.Length];
			}

			m_valueArray = newArray;
			m_head = 0;
		}
	}
}
