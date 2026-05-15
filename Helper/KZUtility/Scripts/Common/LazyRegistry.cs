using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	public sealed class LazyRegistry<TKey,TValue>
	{
		public delegate bool ValueProvider(TKey key,out TValue result);

		private readonly Dictionary<TKey,TValue> m_storageDict = new();

		public TValue Fetch(TKey key,ValueProvider onProvider)
		{
			if(!m_storageDict.TryGetValue(key,out var instance))
			{
				if(!onProvider(key,out instance))
				{
					throw new InvalidOperationException($"Failed to resolve value for key: {key}");
				}

				m_storageDict.Add(key,instance);
			}

			return instance;
		}

		public void Revoke(TKey key)
		{
			if(m_storageDict.ContainsKey(key))
			{
				m_storageDict.Remove(key);
			}
		}

		public void Release()
		{
			foreach(var pair in m_storageDict)
			{
				var storage = pair.Value;

				if(storage is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}

			m_storageDict.Clear();
		}
	}
}