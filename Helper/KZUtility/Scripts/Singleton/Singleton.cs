using System;

namespace KZLib.Utilities
{
	public abstract class Singleton<TClass> : IDisposable where TClass : class
	{
		private static readonly object m_syncRoot = new();
		private static TClass? s_instance = null;
		private bool m_disposed = false;

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
		~Singleton() => _Release(false);

		protected virtual void _Initialize() { }

		protected virtual void _Release(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing) { }

			m_disposed = true;
			s_instance = null;
		}

		public void Dispose()
		{
			_Release(true);

			GC.SuppressFinalize(this);
		}

		protected void _ThrowIfDisposed()
		{
			if(m_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}