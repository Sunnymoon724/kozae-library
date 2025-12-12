using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace KZConsole.KZUtility
{
    public static class CommonUtility
	{
		public static Dictionary<string, string> ReadEmbeddedResourcesFromExtension(Assembly assembly, string extension)
		{
			var resourceNameArray = assembly.GetManifestResourceNames();
			var resourceDict = new Dictionary<string,string>();

			for(var i=0;i<resourceNameArray.Length;i++)
			{
				var resourceName = resourceNameArray[i];

				if(resourceName.EndsWith(extension))
				{
					using var stream = assembly.GetManifestResourceStream(resourceName);

					if(stream == null)
					{
						continue;
					}

					using var streamReader = new StreamReader(stream);
					string content = streamReader.ReadToEnd();

					var key = _GetFileName(resourceName);

					resourceDict.Add(key,content);
				}
			}

			return resourceDict;
		}
		
		private static string _GetFileName(string resourceName)
		{
			var resourceArray = resourceName.Split('.');

			if(resourceArray.Length >= 2)
			{
				return $"{resourceArray[^2]}.{resourceArray[^1]}";
			}

			return resourceName;
		}

		public static string ReadEmbeddedResourceFile(Assembly assembly, string fileName)
		{
			using var stream = assembly.GetManifestResourceStream(fileName) ?? throw new FileNotFoundException($"Resource not found. [{fileName}]");
			using var streamReader = new StreamReader(stream);

			return streamReader.ReadToEnd();
		}
	}
}