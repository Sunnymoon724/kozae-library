using System.Collections.Generic;
using KZConsole.Utilities;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> protoFolderAbsolutePath / 1 -> environment (output: ../ProtoOutput from exe parent folder)
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,2,"KZProtoExtractor <protoFolderAbsolutePath> <environment>",onPlayProgram);
		}

		/// <summary>
		/// extract byte file
		/// </summary>
		/// <param name="argumentArray"></param>
		private static void onPlayProgram(string[] argumentArray)
		{
			var protoFolderPath = argumentArray[0];

			KZCommonKit.WriteLog($"Proto folder path : {protoFolderPath}",LogType.Info);

			var protoFilePathList  = new List<string>(KZFileKit.FindExcelFilesInFolder(protoFolderPath));

			var environment = argumentArray[1];
			var branchFilePath = KZFileKit.FindPathByFileName(protoFilePathList,"Branch");

			KZCommonKit.WriteLog($"Environment : {environment}",LogType.Info);

			var protoExtractor = new ProtoExtractor(environment,branchFilePath);

			protoExtractor.ExtractAllProto(protoFilePathList);
		}
	}
}