using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	/// <summary>
	/// Thread-safe keyed cache with lazy resolution. The first <see cref="Fetch"/> for a key
	/// invokes a provider; later calls return the cached value.
	/// </summary>
	/// <typeparam name="TKey">Cache key type.</typeparam>
	/// <typeparam name="TValue">Cached value type.</typeparam>
	public sealed class LazyRegistry<TKey,TValue>
	{
		/// <summary>
		/// Resolves a value for the given key. Return false when the key cannot be resolved.
		/// </summary>
		public delegate bool ValueProvider(TKey key,out TValue result);

		private readonly object m_syncRoot = new();
		private readonly Dictionary<TKey,TValue> m_storageDict = new();

		/// <summary>
		/// Returns the cached value for <paramref name="key"/>, or resolves and stores it via <paramref name="onProvider"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="onProvider"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the provider fails to resolve the key.</exception>
		public TValue Fetch(TKey key,ValueProvider onProvider)
		{
			if(onProvider == null)
			{
				throw new ArgumentNullException(nameof(onProvider));
			}

			lock(m_syncRoot)
			{
				if(m_storageDict.TryGetValue(key,out var instance))
				{
					return instance;
				}

				if(!onProvider(key,out instance))
				{
					throw new InvalidOperationException($"Failed to resolve value for key: {key}");
				}

				m_storageDict[key] = instance;

				return instance;
			}
		}

		/// <summary>
		/// Removes a single cached entry. Disposes the value when it implements <see cref="IDisposable"/>.
		/// </summary>
		public void Revoke(TKey key)
		{
			lock(m_syncRoot)
			{
				if(m_storageDict.Remove(key,out var storage))
				{
					_DisposeIfNeeded(storage);
				}
			}
		}

		/// <summary>
		/// Disposes all cached values that implement <see cref="IDisposable"/>, then clears the registry.
		/// </summary>
		public void Release()
		{
			lock(m_syncRoot)
			{
				foreach(var pair in m_storageDict)
				{
					_DisposeIfNeeded(pair.Value);
				}

				m_storageDict.Clear();
			}
		}

		private static void _DisposeIfNeeded(TValue value)
		{
			if(value is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}
}