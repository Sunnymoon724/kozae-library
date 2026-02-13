using System;
using System.IO;
using KZConsole.Utilities;
using KZLib.Utilities;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> luaFolderPath / 1 -> resultFolderPath
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,onPlayProgram);
		}
		
		private static void onPlayProgram(string[] argumentArray)
		{
			var currentPath = Directory.GetCurrentDirectory();
			var luaFolderPath = Path.GetFullPath(Path.Combine(currentPath,argumentArray[0]));

			CommonUtility.WriteLog($"Lua folder path : {luaFolderPath}",LogType.Info);

			var resultFolderPath = argumentArray[1];

			FileUtility.CreateFolder(resultFolderPath);

			foreach(var filePath in FileUtility.GetFilePathArray(luaFolderPath))
			{
				var newFilePath = filePath.Replace(luaFolderPath,resultFolderPath);

				newFilePath = FileUtility.ChangeExtension(newFilePath,".lua.bytes");

				FileUtility.CreateFolder(newFilePath);
				FileUtility.CopyFile(filePath,newFilePath,true);
			}
		}
	}
}