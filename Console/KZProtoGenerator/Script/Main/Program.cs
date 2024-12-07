
namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> protoFolderPath / 1 -> branchName
		/// </summary>
		/// <param name="argumentArray"></param>
		private static void Main(string[] argumentArray)
		{
			try
			{
				//? Create Project
				var currentPath = Directory.GetCurrentDirectory();
				var protoFolderPath = GetFullPath(currentPath,argumentArray[0]);

				var protoFilePathList = new List<string>(GetExcelFilePathGroup(protoFolderPath));

				var enumFilePath = GetFilePath(protoFilePathList,"Enum");
				var branchFilePath = GetFilePath(protoFilePathList,"Branch");

				var codeGenerator = new CodeGenerator(protoFilePathList,enumFilePath);
				var codeList = codeGenerator.GenerateAllProtoCode();

				var outputFolderPath = GetFullPath(currentPath,"../ProtoOutput");
				var protoGenerator = new ProtoGenerator(argumentArray[1],branchFilePath);

				protoGenerator.GenerateAllProto(protoFilePathList,codeList,outputFolderPath);
			}
			catch(Exception exception)
			{
				Console.WriteLine($"Error: {exception.Message} / Stack Trace: {exception.StackTrace}");
			}

			Console.WriteLine("Press enter to exit...");
			Console.ReadLine();
		}

		private static string GetFilePath(List<string> filePathList,string text)
		{
			var filePath = filePathList.Find(x => x.Contains(text));

			if(string.IsNullOrEmpty(filePath))
			{
				throw new NullReferenceException($"{text} excel file does not exist.");
			}

			return filePath;
		}

		private static IEnumerable<string> GetExcelFilePathGroup(string folderPath)
		{
			IsFolderExist(folderPath);

			foreach(var extension in new string[] { "*.xls", "*.xlsx", "*.xlsm" })
			{
				foreach(var filePath in Directory.GetFiles(folderPath,extension))
				{
					yield return filePath;
				}
			}
		}

		private static bool IsFolderExist(string folderPath)
		{
			if(string.IsNullOrEmpty(folderPath))
			{
				throw new NullReferenceException("FolderPath is null");
			}

			var result = Directory.Exists(folderPath);

			if(!result)
			{
				throw new NullReferenceException($"{folderPath} is not exist");
			}

			return result;
		}

		private static string GetFullPath(params string[] pathArray)
		{
			var path = Path.Combine(pathArray);

			if(string.IsNullOrEmpty(path))
			{
				throw new NullReferenceException("Path is null");
			}

			return Path.GetFullPath(path);
		}
	}
}