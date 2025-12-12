using System;
using System.Collections.Generic;
using System.IO;
using KZLib.KZUtility;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> protoFolderAbsolutePath / 1 -> projectPluginAbsolutePath
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,onPlayProgram);
		}

		/// <summary>
		/// create project -> generate KZProto code file -> build project -> move file to project -> clean up
		/// </summary>
		private static void onPlayProgram(string[] argumentArray)
		{
			var protoFolderAbsolutePath = argumentArray[0];
			var projectPluginAbsolutePath = argumentArray[1];

			Console.WriteLine($"Proto folder path : {protoFolderAbsolutePath}");

			var protoFilePathList = new List<string>(FileUtility.FindAllExcelFileGroupByFolderPath(protoFolderAbsolutePath));

			var parentPath = FileUtility.GetProjectParentPath();
			var projectFolderPath = Path.Combine(parentPath,"ProtoProject");
			
			var projectManager = new ProjectManager(projectFolderPath);

			projectManager.CreateProject();

			var builder = new ProtoBuilder(protoFilePathList,projectFolderPath);

			builder.GenerateAllProtoCode();

			Console.WriteLine("Build project");

			//? Build Project
			projectManager.BuildProject();

			Console.WriteLine("Move dll & pdb file");
			
			Console.WriteLine($"project plugin path : {projectPluginAbsolutePath}");

			FileUtility.CreateFolder(projectPluginAbsolutePath);

			var outputFolderPath = Path.Combine(parentPath,"ProtoOutput","Plugin");

			var sourceDllFilePath = Path.Combine(outputFolderPath,"KZProto.dll");
			var sourcePdbFilePath = Path.Combine(outputFolderPath,"KZProto.pdb");

			FileUtility.MoveFile(sourceDllFilePath,projectPluginAbsolutePath,true);
			FileUtility.MoveFile(sourcePdbFilePath,projectPluginAbsolutePath,true);

			Console.WriteLine("Delete project");

			//? Delete Project
			FileUtility.DeleteFolder(projectFolderPath,true);
		}
	}
}