using System.Threading;

namespace KZLib.Utilities
{
	public class TransientStore<TTransient> where TTransient : class
	{
		private static TTransient? s_transient = null;

		public static bool Set(TTransient transient,bool isForce = false)
		{
			if(isForce)
			{
				Interlocked.Exchange(ref s_transient,transient);

				return true;
			}

			var original = Interlocked.CompareExchange(ref s_transient,transient,null);

			return original == null;
		}

		public static TTransient Consume()
		{
			return Interlocked.Exchange(ref s_transient,null)!;
		}
	}
}