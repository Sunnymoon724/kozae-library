
namespace KZConsole
{
	internal class Utility
    {
		internal static bool IsPathExist(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				throw new NullReferenceException("Path is null");
			}

			return true;
		}
	}
}