using System;
using System.Collections.Generic;
using System.IO;
using KZLib.KZUtility;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> protoFolderPath / 1 -> branchName / 2 -> resultFolderPath
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,onPlayProgram);
		}
		
		private static void onPlayProgram(string[] argumentArray)
		{
			var currentPath = Directory.GetCurrentDirectory();
			var protoFolderPath = Path.GetFullPath(Path.Combine(currentPath,argumentArray[0]));

			Console.WriteLine($"Proto folder path : {protoFolderPath}");

			var outputFolderPath = Path.GetFullPath(Path.Combine(currentPath,"../ProtoOutput"));

			var projectManager = new ProjectManager(currentPath,outputFolderPath);

			projectManager.CreateProject();

			var protoFilePathList = new List<string>(FileUtility.FindAllExcelFileGroupByFolderPath(protoFolderPath));

			var enumFilePath = FileUtility.FindFilePath(protoFilePathList,"Enum");
			var branchFilePath = FileUtility.FindFilePath(protoFilePathList,"Branch");

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
			var resultFolderPath = Path.GetFullPath(argumentArray[2]);

			FileUtility.CreateFolder(resultFolderPath);

			var sourceDllFilePath = Path.Combine(outputFolderPath,"KZProto.dll");
			var sourcePdbFilePath = Path.Combine(outputFolderPath,"KZProto.pdb");

			FileUtility.MoveFile(sourceDllFilePath,resultFolderPath,true);
			FileUtility.MoveFile(sourcePdbFilePath,resultFolderPath,true);
		}
	}
}