using System;
using System.Collections.Generic;
using KZLib.KZUtility;

namespace KZConsole.KZProto
{
	public class Program
	{
		/// <summary>
		/// 0 -> protoFolderAbsolutePath / 1 -> environment / 2 -> outputFolderPath
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,onPlayProgram);
		}

		/// <summary>
		/// extract byte file
		/// </summary>
		/// <param name="argumentArray"></param>
		private static void onPlayProgram(string[] argumentArray)
		{
			var protoFolderPath = argumentArray[0];

			Console.WriteLine($"Proto folder path : {protoFolderPath}");

			var protoFilePathList  = new List<string>(FileUtility.FindAllExcelFileGroupByFolderPath(protoFolderPath));

			var environment = argumentArray[1];
			var branchFilePath = FileUtility.FindFilePath(protoFilePathList,"Branch");

			Console.WriteLine($"Environment : {environment}");

			var protoExtractor = new ProtoExtractor(environment,branchFilePath);

			protoExtractor.ExtractAllProto(protoFilePathList);
		}
	}
}