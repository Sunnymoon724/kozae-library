using System.Globalization;
using KZLib.KZUtility;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> luaFolderPath / 1 -> resultFolderPath
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				var currentPath = Directory.GetCurrentDirectory();
				var luaFolderPath = Path.GetFullPath(Path.Combine(currentPath,argumentArray[0]));

				Console.WriteLine($"Lua folder path : {luaFolderPath}");

				var resultFolderPath = argumentArray[1];

				FileUtility.CreateFolder(resultFolderPath);

				foreach(var filePath in FileUtility.GetFilePathArray(luaFolderPath))
				{
					var newFilePath = filePath.Replace(luaFolderPath,resultFolderPath);

					newFilePath = FileUtility.ChangeExtension(newFilePath,".lua.bytes");

					FileUtility.CreateFolder(newFilePath);

					FileUtility.CopyFile(filePath,newFilePath,true);
				}

				Console.WriteLine("Press enter to exit...");
				Console.ReadLine();
			}
			catch(Exception exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{exception}");
				Console.ResetColor();

				Environment.Exit(-1);
			}
		}
	}
}