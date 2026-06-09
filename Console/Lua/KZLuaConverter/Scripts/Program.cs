using System;
using System.IO;
using KZConsole.Utilities;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> luaFolderRelativePath / 1 -> resultFolderRelativePath (relative to exe working directory)
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,2,"KZLuaConverter <luaFolderRelativePath> <resultFolderRelativePath>",onPlayProgram);
		}

		private static void onPlayProgram(string[] argumentArray)
		{
			var currentPath = KZFileKit.GetProjectPath();
			var luaFolderPath = Path.GetFullPath(Path.Combine(currentPath,argumentArray[0]));
			var resultFolderPath = Path.GetFullPath(Path.Combine(currentPath,argumentArray[1]));

			KZCommonKit.WriteLog($"Lua folder path : {luaFolderPath}",LogType.Info);
			KZCommonKit.WriteLog($"Result folder path : {resultFolderPath}",LogType.Info);

			if(!KZFileKit.IsFolderExist(luaFolderPath))
			{
				throw new DirectoryNotFoundException($"Lua folder not found: {luaFolderPath}");
			}

			KZFileKit.CreateFolder(resultFolderPath);

			KZCommonKit.WriteLog("Convert lua files.",LogType.Info);

			var convertedCount = 0;

			foreach(var filePath in KZFileKit.FindFilePathGroup(luaFolderPath,true))
			{
				if(!string.Equals(KZFileKit.GetExtension(filePath),".lua",StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				var relativePath = Path.GetRelativePath(luaFolderPath,filePath);
				var destinationFilePath = KZFileKit.ChangeExtension(Path.Combine(resultFolderPath,relativePath),".lua.bytes");

				KZFileKit.CreateFolder(destinationFilePath);
				File.Copy(filePath,destinationFilePath,true);

				KZCommonKit.WriteLog($"-Copy {relativePath}",LogType.Info);

				convertedCount++;
			}

			if(convertedCount == 0)
			{
				KZCommonKit.WriteLog("Warning : No .lua files found.",LogType.Warning);
			}
		}
	}
}
