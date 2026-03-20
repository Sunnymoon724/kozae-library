using System.IO;

namespace KZConsole
{
	public class Program
	{
		private const string c_protoBuilder = "KZProtoBuilder";
		private const string c_protoExtractor = "KZProtoExtractor";

		/// <summary>
		/// 0 -> builderProjectPath / 1 -> extractorProjectPath / 2 -> resultFolderPath
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,onPlayProgram);
		}

		/// <summary>
		/// run builder project -> run extractor project
		/// </summary>
		private static void onPlayProgram(string[] argumentArray)
		{
			var protoFolderRelativePath = argumentArray[0];
			var environment = argumentArray[1];
			var projectPluginRelativePath = argumentArray[2];

			// builder -> extractor -> move & clean
			var currentPath = KZFileKit.GetProjectPath();

			var protoFolderAbsolutePath = Path.GetFullPath(Path.Combine(currentPath,protoFolderRelativePath));
			var projectPluginAbsolutePath = Path.GetFullPath(Path.Combine(currentPath,projectPluginRelativePath));

			var parentPath = KZFileKit.GetProjectParentPath();

			var builderFolderPath = Path.GetFullPath(Path.Combine(parentPath,$"{c_protoBuilder}",$"{c_protoBuilder}.exe"));
			var builderArgument = string.Join(" ",protoFolderAbsolutePath,projectPluginAbsolutePath);

			ProjectRunner.RunProject(c_protoBuilder,builderFolderPath,builderArgument);

			var extractorFolderPath = Path.GetFullPath(Path.Combine(parentPath,$"{c_protoExtractor}",$"{c_protoExtractor}.exe"));
			var extractorArgument = string.Join(" ",protoFolderAbsolutePath,environment);

			ProjectRunner.RunProject(c_protoExtractor,extractorFolderPath,extractorArgument);
		}
	}
}