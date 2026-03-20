using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	public sealed class LazyRegistry<TKey,TValue> where TValue : class
	{
		private readonly Dictionary<TKey,TValue> m_storageDict = new();

		public TValue Fetch(TKey key,Func<TKey,TValue> factory)
		{
			if(!m_storageDict.TryGetValue(key,out var instance))
			{
				instance = factory(key) ?? throw new InvalidOperationException($"Failed to create value for key: {key}");

				m_storageDict.Add(key,instance);
			}

			return instance;
		}

		public void Clear()
		{
			m_storageDict.Clear();
		}
	}
}