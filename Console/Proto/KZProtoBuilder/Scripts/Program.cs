using System.Collections.Generic;
using System.IO;
using KZConsole.Utilities;

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

			KZCommonKit.WriteLog($"Proto folder path : {protoFolderAbsolutePath}",LogType.Info);

			var protoFilePathList = new List<string>(KZFileKit.FindAllExcelFileGroupByFolderPath(protoFolderAbsolutePath));

			var parentPath = KZFileKit.GetProjectParentPath();
			var projectFolderPath = Path.Combine(parentPath,"ProtoProject");
			
			var projectManager = new ProjectManager(projectFolderPath);

			projectManager.CreateProject();

			var builder = new ProtoBuilder(protoFilePathList,projectFolderPath);

			builder.GenerateAllProtoCode();

			KZCommonKit.WriteLog("Build project",LogType.Info);

			//? Build Project
			projectManager.BuildProject();

			KZCommonKit.WriteLog("Move dll & pdb file",LogType.Info);

			KZCommonKit.WriteLog($"project plugin path : {projectPluginAbsolutePath}",LogType.Info);

			KZFileKit.CreateFolder(projectPluginAbsolutePath);

			var outputFolderPath = Path.Combine(parentPath,"ProtoOutput","Plugin");

			var sourceDllFilePath = Path.Combine(outputFolderPath,"KZProto.dll");
			var sourcePdbFilePath = Path.Combine(outputFolderPath,"KZProto.pdb");

			KZFileKit.MoveFile(sourceDllFilePath,projectPluginAbsolutePath,true);
			KZFileKit.MoveFile(sourcePdbFilePath,projectPluginAbsolutePath,true);

			KZCommonKit.WriteLog("Delete project",LogType.Info);

			//? Delete Project
			KZFileKit.DeleteFolder(projectFolderPath,true);
		}
	}
}