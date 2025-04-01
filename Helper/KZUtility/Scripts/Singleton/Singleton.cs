using System;

namespace KZLib.KZUtility
{
	public abstract class Singleton<TClass> : IDisposable where TClass : class,new()
	{
		private static TClass? s_instance = null;

		private bool m_disposed = false;

		public static TClass In
		{
			get
			{
				s_instance ??= new TClass();

				return s_instance;
			}
		}

		public static bool HasInstance => s_instance != null;

		protected Singleton() => Initialize();
		~Singleton() => Release(false);

		protected virtual void Initialize() { }

		protected virtual void Release(bool disposing)
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
			Release(true);
			GC.SuppressFinalize(this);
		}
	}
}