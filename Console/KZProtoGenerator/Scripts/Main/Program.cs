using System.Globalization;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> protoFolderPath / 1 -> branchName / 2 -> resultFolderPath
		/// </summary>
		private static void Main(string[] argumentArray)
		{
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				var currentPath = Directory.GetCurrentDirectory();
				var protoFolderPath = Utility.GetFullPath(currentPath,argumentArray[0]);
				var outputFolderPath = Utility.GetFullPath(currentPath,"../ProtoOutput");

				Utility.CreateFolder(outputFolderPath);

				var projectManager = new ProjectManager(currentPath,outputFolderPath);

				projectManager.CreateProject();

				var protoFilePathList = new List<string>(GetExcelFilePathGroup(protoFolderPath));

				var enumFilePath = GetFilePath(protoFilePathList,"Enum");
				var branchFilePath = GetFilePath(protoFilePathList,"Branch");

				var codeGenerator = new CodeGenerator(protoFilePathList,enumFilePath);
				codeGenerator.GenerateAllProtoCode(projectManager.ProjectFolderPath);

				var protoGenerator = new ProtoGenerator(argumentArray[1],branchFilePath);

				protoGenerator.GenerateAllProto(protoFilePathList,codeGenerator.ProtoCodeGroup,outputFolderPath);

				Console.WriteLine("Build project");

				//? Build Project
				projectManager.BuildProject();

				Console.WriteLine("Delete project");

				//? Delete Project
				projectManager.DeleteProject();

				Console.WriteLine("Move dll & pdb file");
				var resultFolderPath = argumentArray[2];

				Utility.CreateFolder(resultFolderPath);

				var sourceDllFilePath = Path.Combine(outputFolderPath,"KZProto.dll");
				var sourcePdbFilePath = Path.Combine(outputFolderPath,"KZProto.pdb");

				var destinationDllFilePath = Path.Combine(resultFolderPath,"KZProto.dll");
				var destinationPdbFilePath = Path.Combine(resultFolderPath,"KZProto.pdb");

				File.Move(sourceDllFilePath,destinationDllFilePath,true);
				File.Move(sourcePdbFilePath,destinationPdbFilePath,true);

				Console.WriteLine("Press enter to exit...");
				Console.ReadLine();
			}
			catch(Exception exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{exception}");
				Console.ResetColor();

				Environment.Exit(1);
			}
		}

		private static string GetFilePath(List<string> filePathList,string text)
		{
			var filePath = filePathList.Find(x => x.Contains(text)) ?? throw new NullReferenceException($"{text} excel file does not exist.");;

			Utility.IsPathExist(filePath);

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
			Utility.IsPathExist(folderPath);

			var result = Directory.Exists(folderPath);

			if(!result)
			{
				throw new NullReferenceException($"{folderPath} is not exist");
			}

			return result;
		}
	}
}