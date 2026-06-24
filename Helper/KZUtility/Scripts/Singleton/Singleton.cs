using System;

namespace KZLib.Utilities
{
	/// <summary>
	/// Thread-safe singleton base for plain C# types. Use <see cref="In"/> to access the instance.
	/// Subclasses should use a private constructor to prevent external instantiation.
	/// </summary>
	public abstract class Singleton<TClass> : IDisposable where TClass : class
	{
		private static readonly object m_syncRoot = new();
		private static TClass? s_instance = null;
		private bool m_disposed = false;

		/// <summary>
		/// Returns the singleton instance, creating it on first access.
		/// </summary>
		public static TClass In
		{
			get
			{
				if(s_instance == null)
				{
					lock(m_syncRoot)
					{
						s_instance ??= _CreateInstance();
					}
				}

				return s_instance;
			}
		}

		public static bool HasInstance => s_instance != null;

		private static TClass _CreateInstance()
		{
			return Activator.CreateInstance(typeof(TClass),true) as TClass ?? throw new InvalidOperationException($"Failed to create instance of {typeof(TClass).Name}");
		}

		protected Singleton() => _Initialize();

		protected virtual void _Initialize() { }

		protected virtual void _Release(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			m_disposed = true;

			if(disposing)
			{
				s_instance = null;
			}
		}

		public void Dispose()
		{
			_Release(true);
			GC.SuppressFinalize(this);
		}

		protected void _EnsureNotDisposed()
		{
			if(m_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}