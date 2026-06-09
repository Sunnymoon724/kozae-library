using System;
using System.Collections.Generic;
using KZLib.Diagnostics;

namespace KZLib.Utilities
{
	public abstract class StrategyCatalog<TOwner,TKey,TStrategy> where TOwner : class where TKey : Enum
	{
		protected readonly TOwner m_owner = null!;
		private readonly Dictionary<TKey,TStrategy> m_strategyDict = null!;

		protected abstract Dictionary<TKey,TStrategy> _BindStrategy();

		public StrategyCatalog(TOwner owner)
		{
			m_owner = owner;
			m_strategyDict = _BindStrategy() ?? new Dictionary<TKey,TStrategy>();
		}

		public bool TryGetStrategy(TKey key,out TStrategy strategy)
		{
			if(m_strategyDict != null && m_strategyDict.TryGetValue(key,out strategy))
			{
				return true;
			}

			LogBridge.E($"[{typeof(TKey).Name}] {key} is not supported in {GetType().Name}.");

			strategy = default!;

			return false;
		}
	}
}