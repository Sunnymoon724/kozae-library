using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace KZConsole.KZUtility
{
    public static class ProtoUtility
	{

		public static string GetOutputFolderPath()
		{
			var parentPath = GetProjectParentPath();

			return Path.Combine(parentPath,"ProtoOutput");
		}

		public static string GetProjectParentPath()
		{
			return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),".."));
		}
	}
}