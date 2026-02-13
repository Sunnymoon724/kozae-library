using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace KZConsole.Utilities
{
	public enum LogType { Info, Warning, Error, }

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

		public static void WriteLog(string message,LogType logType)
		{
			switch(logType)
			{
				case LogType.Info:
					Console.WriteLine(message);
					break;
				case LogType.Warning:
					_WriteColorLog(message,ConsoleColor.Yellow);
					break;
				case LogType.Error:
					_WriteColorLog(message,ConsoleColor.Red);
					break;
			}
		}

		private static void _WriteColorLog(string message,ConsoleColor consoleColor)
		{
			var tempColor = Console.ForegroundColor;

			Console.ForegroundColor = consoleColor;

			Console.WriteLine(message);

			Console.ForegroundColor = tempColor;
		}
	}
}