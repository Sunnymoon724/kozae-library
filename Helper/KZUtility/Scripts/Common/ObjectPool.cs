using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	/// <summary>
	/// Manage pool of Objects.
	/// </summary>
	public class ObjectPool<TObject> : IDisposable where TObject : class
	{
		private bool m_disposed = false;

		private readonly Queue<TObject> m_poolQueue = null!;
		private readonly TObject m_pivot = null!;
		private readonly Func<TObject,TObject>? m_initializeFunc = null;

		public ObjectPool(Func<TObject,TObject>? initializeFunc,TObject pivot,int capacity) : this(initializeFunc,pivot,capacity,true) { }

		protected ObjectPool(Func<TObject,TObject>? initializeFunc,TObject pivot,int capacity,bool autoFill)
		{
			m_poolQueue = new(capacity);
			m_pivot = pivot ?? throw new ArgumentNullException(nameof(pivot),"Pivot object cannot be null.");
			m_initializeFunc = initializeFunc;

			if(autoFill)
			{
				_Fill(capacity);
			}
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this); 
		}

		protected virtual void Dispose(bool disposing)
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

		protected void _Fill(int capacity)
		{
			for(var i=0;i<capacity;i++)
			{
				var item = _InitializePivot();

				Put(item);
			}
		}

		public virtual void Put(TObject item)
		{
			if(item == null)
			{
				return;
			}

			m_poolQueue.Enqueue(item);
		}

		public virtual TObject GetOrCreate()
		{
			return m_poolQueue.Count > 0 ? m_poolQueue.Dequeue() : _InitializePivot();
		}

		public void Clear(Action<TObject>? onDestroy = null)
		{
			while(m_poolQueue.Count > 0)
			{
				var item = m_poolQueue.Dequeue();

				onDestroy?.Invoke(item);
			}
		}

		private TObject _InitializePivot()
		{
			return m_initializeFunc != null ? m_initializeFunc(m_pivot) : m_pivot;
		}
	}
}