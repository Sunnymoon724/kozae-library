using System.IO;
using KZConsole.Utilities;

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

			KZCommonKit.WriteLog($"Lua folder path : {luaFolderPath}",LogType.Info);

			var resultFolderPath = argumentArray[1];

			KZFileKit.CreateFolder(resultFolderPath);

			foreach(var filePath in KZFileKit.GetFilePathArray(luaFolderPath))
			{
				var newFilePath = filePath.Replace(luaFolderPath,resultFolderPath);

				newFilePath = KZFileKit.ChangeExtension(newFilePath,".lua.bytes");

				KZFileKit.CreateFolder(newFilePath);
				KZFileKit.CopyFile(filePath,newFilePath,true);
			}
		}
	}
}