using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	/// <summary>
	/// Public contract for a pawn checked out from <see cref="PawnPool{TPawn}"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="Create"/> clones new pawns from the constructor pivot.
	/// <see cref="Initialize"/> runs on checkout via <see cref="PawnPool{TPawn}.GetOrCreate"/>; keep it lightweight and do not re-enter the pool.
	/// When <see cref="PawnPool{TPawn}"/> capacity is 1, <see cref="Create"/> may return the pivot itself for single-instance reuse.
	/// </remarks>
	public interface IPawn
	{
		/// <summary>Creates a new pawn from the <paramref name="pivot"/> template.</summary>
		/// <param name="pivot">Template pawn to clone.</param>
		IPawn Create(IPawn pivot);

		/// <summary>Prepares the pawn on checkout. Invoked by <see cref="PawnPool{TPawn}.GetOrCreate"/> outside the pool lock.</summary>
		void Initialize();
	}

	/// <summary>Optional teardown hooks invoked by <see cref="PawnPool{TPawn}"/>.</summary>
	/// <remarks>
	/// <see cref="Release"/> runs outside the pool lock during <see cref="PawnPool{TPawn}.Put"/> and must be idempotent.
	/// <see cref="Destroy"/> runs when idle capacity is exceeded, during <see cref="PawnPool{TPawn}.Clear"/>, or on dispose.
	/// </remarks>
	public interface IPawnHook
	{
		/// <summary>Resets the pawn after checkout. Must be safe to call more than once.</summary>
		void Release();

		/// <summary>Tears down pawn resources when it is discarded or the pool is cleared.</summary>
		void Destroy();
	}

	/// <summary>
	/// Queue-backed pawn pool. Reuse idle pawns via <see cref="GetOrCreate"/> and <see cref="Put"/>.
	/// </summary>
	/// <typeparam name="TPawn">Pawn implementation that implements <see cref="IPawn"/>.</typeparam>
	/// <remarks>
	/// Unlike <see cref="LanePool{TLane,TPayload}"/>, which tracks concurrently active lanes, only idle pawns are stored in the queue.
	/// <see cref="m_capacity"/> bounds idle queue size, not concurrent checkouts; extra checkouts create new pawns on demand.
	/// <see cref="IPawn.Initialize"/> runs outside the pool lock during <see cref="GetOrCreate"/>.
	/// <see cref="IPawnHook.Release"/> runs outside the pool lock during <see cref="Put"/>.
	/// Pawns returned when the idle queue is full are released and destroyed.
	/// When capacity is 1, <see cref="IPawn.Create"/> may return the pivot pawn itself for single-instance reuse.
	/// <see cref="Clear"/> destroys idle pawns while keeping the pool reusable.
	/// <see cref="Dispose"/> performs <see cref="_Clear"/> and marks the pool as disposed.
	/// Checked-out pawns are not tracked; return them with <see cref="Put"/> before <see cref="Clear"/> or <see cref="Dispose"/>.
	/// The constructor <paramref name="pivot"/> is a factory template and is not destroyed by the pool.
	/// Extend pawn creation by overriding <see cref="_Create"/> or <see cref="_Spawn"/>.
	/// </remarks>
	public class PawnPool<TPawn> : IDisposable where TPawn : class,IPawn
	{
		/// <summary>Synchronizes idle queue state.</summary>
		private readonly object m_syncRoot = new();
		/// <summary>Idle pawns waiting to be checked out by <see cref="GetOrCreate"/>.</summary>
		private readonly Queue<TPawn> m_poolQueue = null!;
		/// <summary>Factory pawn passed to the constructor; not enqueued. Invokes <see cref="IPawn.Create"/> during <see cref="_Create"/>.</summary>
		private readonly TPawn m_pivot = default!;
		/// <summary>Maximum number of idle pawns kept in <see cref="m_poolQueue"/>.</summary>
		private readonly int m_capacity = 0;

		private bool m_disposed = false;

		/// <summary>Creates a pool and pre-fills it with <paramref name="capacity"/> idle pawns.</summary>
		/// <param name="pivot">Template pawn passed to <see cref="IPawn.Create"/>.</param>
		/// <param name="capacity">Maximum number of idle pawns kept in the pool.</param>
		public PawnPool(TPawn pivot,int capacity) : this(pivot,capacity,true) { }

		/// <summary>
		/// Creates a pool. When <paramref name="autoFill"/> is true, pre-fills the pool up to <paramref name="capacity"/> idle pawns.
		/// </summary>
		/// <param name="pivot">Template pawn passed to <see cref="IPawn.Create"/>.</param>
		/// <param name="capacity">Maximum number of idle pawns kept in the pool.</param>
		/// <param name="autoFill">When true, calls <see cref="_Prepare"/> during construction.</param>
		protected PawnPool(TPawn pivot,int capacity,bool autoFill)
		{
			if(capacity <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity),capacity,"Capacity must be greater than zero.");
			}

			m_poolQueue = new Queue<TPawn>(capacity);
			m_pivot = pivot ?? throw new ArgumentNullException(nameof(pivot),"Pivot pawn cannot be null.");
			m_capacity = capacity;

			_ValidateCreate(pivot);

			if(autoFill)
			{
				_Prepare(capacity);
			}
		}

		/// <summary>Destroys idle pawns via <see cref="_Clear"/> and marks the pool as disposed.</summary>
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

		/// <summary>Pre-fills the pool by calling <see cref="IPawn.Create"/> on the factory pawn.</summary>
		/// <param name="capacity">Number of idle pawns to create and enqueue.</param>
		protected void _Prepare(int capacity)
		{
			lock(m_syncRoot)
			{
				_EnsureNotDisposed();

				for(var i=0;i<capacity;i++)
				{
					var item = _Create();

					_TryEnqueue(item);
				}
			}
		}

		/// <summary>
		/// Returns a pawn to the pool. <see cref="IPawnHook.Release"/> runs outside the pool lock.
		/// When the idle queue is full, the pawn is released and destroyed instead of being stored.
		/// </summary>
		public virtual void Put(TPawn pawn)
		{
			if(pawn == null)
			{
				throw new ArgumentNullException(nameof(pawn));
			}

			_EnsureNotDisposed();

			if(pawn is IPawnHook releaseHook)
			{
				releaseHook.Release();
			}

			var discarded = false;

			lock(m_syncRoot)
			{
				_EnsureNotDisposed();

				discarded = !_TryEnqueue(pawn);
			}

			if(discarded && pawn is IPawnHook destroyHook)
			{
				destroyHook.Destroy();
			}
		}

		/// <summary>Returns an idle pawn when available, or creates a new one via <see cref="IPawn.Create"/>.</summary>
		public virtual TPawn GetOrCreate()
		{
			TPawn pawn = null!;

			lock(m_syncRoot)
			{
				_EnsureNotDisposed();

				pawn = m_poolQueue.Count > 0 ? m_poolQueue.Dequeue() : _Create();
			}

			pawn.Initialize();

			return pawn;
		}

		/// <summary>Destroys all idle pawns currently stored in the pool. Call <see cref="_Prepare"/> to pre-fill again (optional).</summary>
		public void Clear()
		{
			_EnsureNotDisposed();

			_Clear();
		}

		/// <summary>Destroys idle pawns in the queue without checking disposal state. Used by <see cref="Dispose"/>.</summary>
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

			for(var i=0;i<pawnList.Count;i++)
			{
				_DestroyPawn(pawnList[i]);
			}
		}

		/// <summary>Creates a pawn from the factory pivot via <see cref="IPawn.Create"/>.</summary>
		protected virtual TPawn _Create()
		{
			var clone = m_pivot.Create(m_pivot);

			return _Spawn(clone);
		}

		/// <summary>Converts the clone returned by <see cref="IPawn.Create"/> into <typeparamref name="TPawn"/>.</summary>
		protected virtual TPawn _Spawn(IPawn clone)
		{
			return clone as TPawn ?? throw new InvalidOperationException("Create must return a non-null pawn.");
		}

		/// <summary>Enqueues <paramref name="pawn"/> when the idle queue is below <see cref="m_capacity"/>.</summary>
		/// <returns>True when the pawn was stored; false when the idle queue is full.</returns>
		private bool _TryEnqueue(TPawn pawn)
		{
			if(m_poolQueue.Count >= m_capacity)
			{
				return false;
			}

			m_poolQueue.Enqueue(pawn);

			return true;
		}

		/// <summary>Verifies that <see cref="IPawn.Create"/> returns distinct pawns when <see cref="m_capacity"/> is greater than 1.</summary>
		/// <param name="pawn">Template pawn supplied to the constructor.</param>
		private void _ValidateCreate(TPawn pawn)
		{
			if(m_capacity <= 1)
			{
				return;
			}

			var first = _Spawn(m_pivot.Create(pawn));
			var second = _Spawn(m_pivot.Create(pawn));

			var isValid = !ReferenceEquals(first,pawn) && !ReferenceEquals(second,pawn) && !ReferenceEquals(first,second);

			if(!ReferenceEquals(first,pawn))
			{
				_DestroyPawn(first);
			}

			if(!ReferenceEquals(second,pawn) && !ReferenceEquals(second,first))
			{
				_DestroyPawn(second);
			}

			if(!isValid)
			{
				throw new ArgumentException("Create must return distinct pawns when capacity is greater than 1.",nameof(pawn));
			}
		}

		/// <summary>Tears down a probe pawn created during <see cref="_ValidateCreate"/>.</summary>
		private void _DestroyPawn(TPawn pawn)
		{
			if(pawn is IPawnHook pawnHook)
			{
				pawnHook.Destroy();
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