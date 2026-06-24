using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	/// <summary>
	/// Marker for pawns managed by <see cref="PawnPool{TPawn}"/>.
	/// </summary>
	/// <remarks>
	/// Copy and teardown are supplied to <see cref="PawnPool{TPawn}"/> via <c>onCopy</c> and <c>onDestroy</c>.
	/// Checkout and return lifecycle hooks may be added by subclassing <see cref="PawnPool{TPawn}"/> and overriding <see cref="PawnPool{TPawn}.GetOrCreate"/> or <see cref="PawnPool{TPawn}.Put"/>.
	/// </remarks>
	public interface IPawn { }

	/// <summary>
	/// Queue-backed pawn pool. Reuse idle pawns via <see cref="GetOrCreate"/> and <see cref="Put"/>.
	/// </summary>
	/// <typeparam name="TPawn">Pawn implementation that implements <see cref="IPawn"/>.</typeparam>
	/// <remarks>
	/// <see cref="GetOrCreate"/> dequeues an idle pawn or copies a new one from the constructor pivot when the queue is empty; there is no checkout cap.
	/// <see cref="Put"/> enqueues the pawn back into the idle queue.
	/// <c>capacity</c> controls optional pre-fill via <c>autoFill</c>, validates distinct copy results when greater than 1, and is the minimum idle count kept after <see cref="_Purge"/>.
	/// <see cref="Clear"/> tears down idle pawns via <c>onDestroy</c> while keeping the pool reusable.
	/// <see cref="PurgeForce"/> immediately purges idle pawns above <c>capacity</c> via <see cref="_Purge"/>.
	/// <see cref="Dispose"/> clears idle pawns and marks the pool as disposed.
	/// Checked-out pawns are not tracked; return them with <see cref="Put"/> before <see cref="Clear"/> or <see cref="Dispose"/>.
	/// The constructor <paramref name="pivot"/> is a copy template and is not destroyed by the pool.
	/// </remarks>
	public class PawnPool<TPawn> : IDisposable where TPawn : class,IPawn
	{
		/// <summary>Synchronizes idle queue state.</summary>
		private readonly object m_syncRoot = new();
		/// <summary>Idle pawns waiting to be checked out by <see cref="GetOrCreate"/>.</summary>
		private readonly Queue<TPawn> m_poolQueue = null!;
		/// <summary>Copy template passed to the constructor; not enqueued and not destroyed by the pool.</summary>
		private readonly TPawn m_pivot = default!;
		/// <summary>Idle pawns to pre-create in <see cref="_Prepare"/>, the threshold used by <see cref="_ValidateCreate"/>, and the minimum idle count kept after <see cref="_Purge"/>.</summary>
		private readonly int m_capacity = 0;
		/// <summary>Minimum initial capacity for <see cref="m_poolQueue"/> when <see cref="m_capacity"/> is small.</summary>
		private const int c_minCapacity = 4;
		/// <summary>Copies a new pawn from the constructor pivot.</summary>
		private readonly Func<TPawn,TPawn> m_onCopy = null!;
		/// <summary>Tears down a pawn removed from the idle queue.</summary>
		private readonly Action<TPawn> m_onDestroy = null!;

		private bool m_disposed = false;

		/// <summary>
		/// Creates a pool. When <paramref name="autoFill"/> is true, pre-fills idle pawns up to <paramref name="capacity"/>.
		/// </summary>
		/// <param name="pivot">Template pawn passed to <paramref name="onCopy"/>.</param>
		/// <param name="capacity">Idle pawns to pre-create when <paramref name="autoFill"/> is true, and the minimum idle count kept after <see cref="_Purge"/>.</param>
		/// <param name="onCopy">Copies a new pawn from the pivot template. Must return distinct instances when <paramref name="capacity"/> is greater than 1.</param>
		/// <param name="onDestroy">Tears down a pawn removed from the idle queue during <see cref="Clear"/>, <see cref="PurgeForce"/>, or <see cref="Dispose"/>.</param>
		/// <param name="autoFill">When true, calls <see cref="_Prepare"/> during construction.</param>
		public PawnPool(TPawn pivot,int capacity,Func<TPawn,TPawn> onCopy,Action<TPawn> onDestroy,bool autoFill = true)
		{
			m_onCopy = onCopy ?? throw new ArgumentNullException(nameof(onCopy),"onCopy cannot be null.");
			m_onDestroy = onDestroy ?? throw new ArgumentNullException(nameof(onDestroy),"onDestroy cannot be null.");

			if(capacity < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity),capacity,"Capacity cannot be negative.");
			}

			m_poolQueue = new Queue<TPawn>(Math.Max(capacity,c_minCapacity));
			m_pivot = pivot ?? throw new ArgumentNullException(nameof(pivot),"Pivot pawn cannot be null.");
			m_capacity = capacity;

			_ValidateCreate(pivot);

			if(autoFill && capacity > 0)
			{
				_Prepare(capacity);
			}
		}

		/// <summary>Destroys idle pawns and marks the pool as disposed.</summary>
		public void Dispose()
		{
			_Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Destroys idle pawns when disposing and marks the pool as disposed.</summary>
		/// <param name="disposing">When true, destroys idle pawns via <see cref="_Clear"/>.</param>
		protected virtual void _Dispose(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				_Clear();
			}

			m_disposed = true;
		}

		/// <summary>Pre-fills idle pawns by copying from <see cref="m_pivot"/>.</summary>
		/// <param name="count">Number of idle pawns to copy and enqueue.</param>
		protected void _Prepare(int count)
		{
			lock(m_syncRoot)
			{
				_EnsureNotDisposed();

				for(var i=0;i<count;i++)
				{
					m_poolQueue.Enqueue(_Copy());
				}
			}
		}

		/// <summary>Returns a pawn to the idle queue.</summary>
		/// <param name="pawn">Pawn to enqueue. Must not be null.</param>
		public virtual void Put(TPawn pawn)
		{
			if(pawn == null)
			{
				throw new ArgumentNullException(nameof(pawn));
			}

			lock(m_syncRoot)
			{
				_EnsureNotDisposed();

				m_poolQueue.Enqueue(pawn);
			}
		}

		/// <summary>Returns an idle pawn when available, or copies a new one from <see cref="m_pivot"/>.</summary>
		public virtual TPawn GetOrCreate()
		{
			lock(m_syncRoot)
			{
				_EnsureNotDisposed();

				return m_poolQueue.Count > 0 ? m_poolQueue.Dequeue() : _Copy();
			}
		}

		/// <summary>Destroys all idle pawns currently stored in the pool. Call <see cref="_Prepare"/> to pre-fill again (optional).</summary>
		public void Clear()
		{
			_EnsureNotDisposed();

			_Clear();
		}

		/// <summary>Dequeues idle pawns under the pool lock and tears them down via <c>onDestroy</c> outside the lock.</summary>
		protected virtual void _Clear()
		{
			var pawnList = new List<TPawn>();

			lock(m_syncRoot)
			{
				while(m_poolQueue.Count > 0)
				{
					pawnList.Add(m_poolQueue.Dequeue());
				}
			}

			_DestroyList(pawnList);
		}

		/// <summary>Invokes <see cref="m_onCopy"/> with <see cref="m_pivot"/>.</summary>
		private TPawn _Copy()
		{
			return m_onCopy(m_pivot);
		}

		/// <summary>Invokes <see cref="m_onDestroy"/>.</summary>
		private void _Destroy(TPawn pawn)
		{
			m_onDestroy(pawn);
		}

		/// <summary>Immediately purges idle pawns above <see cref="m_capacity"/> via <see cref="_Purge"/>.</summary>
		public virtual void PurgeForce()
		{
			_EnsureNotDisposed();

			_Purge();
		}

		/// <summary>
		/// Dequeues idle pawns while the queue count exceeds <see cref="m_capacity"/> under the pool lock and tears them down via <see cref="_DestroyList"/> outside the lock.
		/// Called by <see cref="PurgeForce"/>; subclasses may also invoke this from a timed purge hook.
		/// </summary>
		protected void _Purge()
		{
			var purgePawnList = new List<TPawn>();

			lock(m_syncRoot)
			{
				while(m_poolQueue.Count > m_capacity)
				{
					purgePawnList.Add(m_poolQueue.Dequeue());
				}
			}

			_DestroyList(purgePawnList);
		}

		/// <summary>Verifies that copy returns distinct pawns when <see cref="m_capacity"/> is greater than 1.</summary>
		/// <param name="pawn">Template pawn supplied to the constructor.</param>
		private void _ValidateCreate(TPawn pawn)
		{
			if(m_capacity <= 1)
			{
				return;
			}

			var first = _Copy();
			var second = _Copy();

			var isValid = !ReferenceEquals(first,pawn) && !ReferenceEquals(second,pawn) && !ReferenceEquals(first,second);

			if(!ReferenceEquals(first,pawn))
			{
				_Destroy(first);
			}

			if(!ReferenceEquals(second,pawn) && !ReferenceEquals(second,first))
			{
				_Destroy(second);
			}

			if(!isValid)
			{
				throw new ArgumentException("onCopy must return distinct pawns when capacity is greater than 1.",nameof(pawn));
			}
		}

		/// <summary>Tears down pawns via <see cref="m_onDestroy"/>.</summary>
		protected void _DestroyList(List<TPawn> pawnList)
		{
			for(var i=0;i<pawnList.Count;i++)
			{
				_Destroy(pawnList[i]);
			}
		}

		/// <summary>Throws <see cref="ObjectDisposedException"/> when the pool has been disposed.</summary>
		private void _EnsureNotDisposed()
		{
			if(m_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}
