using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	/// <summary>
	/// Object pool backed by a queue. Checked-out items are created via <see cref="GetOrCreate"/>
	/// and returned via <see cref="Put"/>.
	/// </summary>
	/// <typeparam name="TObject">Pooled reference type.</typeparam>
	/// <remarks>
	/// When initializeFunc is null, the pool reuses a single pivot instance and capacity must be 1.
	/// Provide initializeFunc to create distinct instances from the pivot template.
	/// </remarks>
	public class ObjectPool<TObject> : IDisposable where TObject : class
	{
		private readonly object m_syncRoot = new();
		private readonly Queue<TObject> m_poolQueue = null!;
		private readonly TObject m_pivot = null!;
		private readonly Func<TObject,TObject>? m_initializeFunc = null;
		private readonly int m_capacity = 0;

		private bool m_disposed = false;

		/// <summary>
		/// Creates a pool and pre-fills it with <paramref name="capacity"/> items.
		/// </summary>
		/// <param name="initializeFunc">Creates a pooled instance from the pivot template. Required when <paramref name="capacity"/> is greater than 1.</param>
		/// <param name="pivot">Template object passed to <paramref name="initializeFunc"/>.</param>
		/// <param name="capacity">Maximum number of idle items kept in the pool.</param>
		public ObjectPool(Func<TObject,TObject>? initializeFunc,TObject pivot,int capacity) : this(initializeFunc,pivot,capacity,true) { }

		/// <summary>
		/// Creates a pool. When <paramref name="autoFill"/> is true, pre-fills the pool up to <paramref name="capacity"/> items.
		/// </summary>
		protected ObjectPool(Func<TObject,TObject>? initializeFunc,TObject pivot,int capacity,bool autoFill)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity),capacity,"Capacity must be greater than zero.");
			}

			if(initializeFunc == null && capacity > 1)
			{
				throw new ArgumentException("Without initializeFunc, capacity must be 1.",nameof(capacity));
			}

			m_poolQueue = new Queue<TObject>(capacity);
			m_pivot = pivot ?? throw new ArgumentNullException(nameof(pivot),"Pivot object cannot be null.");
			m_initializeFunc = initializeFunc;
			m_capacity = capacity;

			if(autoFill)
			{
				_Fill(capacity);
			}
		}

		/// <summary>
		/// Disposes idle pooled items that implement <see cref="IDisposable"/>.
		/// </summary>
		public void Dispose()
		{
			_Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void _Dispose(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				static void _Clear(TObject item)
				{
					if(item is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}

				Clear(_Clear);
			}

			m_disposed = true;
		}

		/// <summary>
		/// Pre-fills the pool with newly initialized items.
		/// </summary>
		protected void _Fill(int capacity)
		{
			lock(m_syncRoot)
			{
				_ThrowIfDisposed();

				for(var i=0;i<capacity;i++)
				{
					var item = _InitializePivot();

					_PutCore(item);
				}
			}
		}

		/// <summary>
		/// Returns an item to the pool. Items beyond the configured capacity are ignored.
		/// </summary>
		public virtual void Put(TObject item)
		{
			if(item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			lock(m_syncRoot)
			{
				_ThrowIfDisposed();
				_PutCore(item);
			}
		}

		/// <summary>
		/// Returns a pooled item when available, or creates a new one from the pivot template.
		/// </summary>
		public virtual TObject GetOrCreate()
		{
			lock(m_syncRoot)
			{
				_ThrowIfDisposed();

				return m_poolQueue.Count > 0 ? m_poolQueue.Dequeue() : _InitializePivot();
			}
		}

		/// <summary>
		/// Removes and processes all idle items currently stored in the pool.
		/// </summary>
		public void Clear(Action<TObject>? onDestroy = null)
		{
			lock(m_syncRoot)
			{
				_ThrowIfDisposed();

				while(m_poolQueue.Count > 0)
				{
					var item = m_poolQueue.Dequeue();

					onDestroy?.Invoke(item);
				}
			}
		}

		private void _PutCore(TObject item)
		{
			if(m_poolQueue.Count >= m_capacity)
			{
				return;
			}

			m_poolQueue.Enqueue(item);
		}

		private TObject _InitializePivot()
		{
			return m_initializeFunc != null ? m_initializeFunc(m_pivot) : m_pivot;
		}

		private void _ThrowIfDisposed()
		{
			if(m_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}
