using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	/// <summary>
	/// Strategy pattern base class. Maps enum keys to strategy implementations
	/// and resolves them through <see cref="TryGetStrategy"/>.
	/// </summary>
	/// <typeparam name="TOwner">Context object passed to subclasses (e.g. the service that owns the strategies).</typeparam>
	/// <typeparam name="TKey">Enum key that identifies each strategy variant.</typeparam>
	/// <typeparam name="TStrategy">Strategy implementation type bound to each key.</typeparam>
	public abstract class StrategyCatalog<TOwner,TKey,TStrategy> where TOwner : class where TKey : Enum
	{
		protected readonly TOwner m_owner = null!;
		private readonly Dictionary<TKey,TStrategy> m_strategyDict = null!;

		/// <summary>
		/// Builds the key-to-strategy map. Called once from the constructor.
		/// </summary>
		protected abstract Dictionary<TKey,TStrategy> _BindStrategy();

		public StrategyCatalog(TOwner owner)
		{
			m_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			m_strategyDict = _BindStrategy() ?? new Dictionary<TKey,TStrategy>();
		}

		/// <summary>
		/// Looks up a strategy by enum key. Returns false when the key is not registered.
		/// </summary>
		public bool TryGetStrategy(TKey key,out TStrategy strategy)
		{
			if(m_strategyDict.TryGetValue(key,out strategy))
			{
				return true;
			}

			strategy = default!;

			return false;
		}
	}
}