using System;
using System.Threading;

namespace KZLib.Utilities
{
	/// <summary>
	/// Thread-safe, process-wide single slot for handing off a value once.
	/// At most one instance per <typeparamref name="TTransient"/> type is held;
	/// <see cref="TryConsume"/> and <see cref="Consume"/> clear the slot after reading.
	/// </summary>
	/// <typeparam name="TTransient">Reference type stored in the slot.</typeparam>
	public class TransientStore<TTransient> where TTransient : class
	{
		private static TTransient? s_transient = null;

		/// <summary>
		/// Stores a value in the slot. Succeeds only when the slot is empty, unless <paramref name="isForce"/> is true.
		/// </summary>
		/// <returns>True when the value was stored; false when the slot was already occupied.</returns>
		public static bool Set(TTransient transient,bool isForce = false)
		{
			if(transient == null)
			{
				throw new ArgumentNullException(nameof(transient));
			}

			if(isForce)
			{
				Interlocked.Exchange(ref s_transient,transient);

				return true;
			}

			var original = Interlocked.CompareExchange(ref s_transient,transient,null);

			return original == null;
		}

		/// <summary>
		/// Atomically reads and clears the slot. Returns null when nothing was stored.
		/// </summary>
		public static TTransient? Consume()
		{
			return TryConsume(out var transient) ? transient : null;
		}

		/// <summary>
		/// Atomically reads and clears the slot when a value is present.
		/// </summary>
		/// <returns>True when a value was consumed; false when the slot was empty.</returns>
		public static bool TryConsume(out TTransient? transient)
		{
			transient = Interlocked.Exchange(ref s_transient,null);

			return transient != null;
		}
	}
}