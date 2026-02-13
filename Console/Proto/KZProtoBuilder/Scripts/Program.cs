using System.Collections.Generic;
using System.IO;
using KZConsole.Utilities;
using KZLib.Utilities;

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

			CommonUtility.WriteLog($"Proto folder path : {protoFolderAbsolutePath}",LogType.Info);

			var protoFilePathList = new List<string>(FileUtility.FindAllExcelFileGroupByFolderPath(protoFolderAbsolutePath));

			var parentPath = FileUtility.GetProjectParentPath();
			var projectFolderPath = Path.Combine(parentPath,"ProtoProject");
			
			var projectManager = new ProjectManager(projectFolderPath);

			projectManager.CreateProject();

			var builder = new ProtoBuilder(protoFilePathList,projectFolderPath);

			builder.GenerateAllProtoCode();

			CommonUtility.WriteLog("Build project",LogType.Info);

			//? Build Project
			projectManager.BuildProject();

			CommonUtility.WriteLog("Move dll & pdb file",LogType.Info);
			
			CommonUtility.WriteLog($"project plugin path : {projectPluginAbsolutePath}",LogType.Info);

			FileUtility.CreateFolder(projectPluginAbsolutePath);

			var outputFolderPath = Path.Combine(parentPath,"ProtoOutput","Plugin");

			var sourceDllFilePath = Path.Combine(outputFolderPath,"KZProto.dll");
			var sourcePdbFilePath = Path.Combine(outputFolderPath,"KZProto.pdb");

			FileUtility.MoveFile(sourceDllFilePath,projectPluginAbsolutePath,true);
			FileUtility.MoveFile(sourcePdbFilePath,projectPluginAbsolutePath,true);

			CommonUtility.WriteLog("Delete project",LogType.Info);

			//? Delete Project
			FileUtility.DeleteFolder(projectFolderPath,true);
		}
	}
}