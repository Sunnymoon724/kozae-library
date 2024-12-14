
namespace KZConsole
{
	public class ProjectManager
	{
		private readonly string m_projectFolderPath = "";
		private readonly string m_outputFolderPath = "";
		private readonly string m_projectFilePath = "";

		public string ProjectFolderPath => m_projectFolderPath;

		public ProjectManager(string currentPath,string outputFolderPath)
		{
			m_projectFolderPath = Utility.GetFullPath(currentPath,"../ProtoProject");
			m_projectFilePath = Path.Combine(m_projectFolderPath,"ProtoProject.csproj");

			Utility.CreateFolder(m_projectFolderPath);

			m_outputFolderPath = outputFolderPath;

			var sourceDllFilePath = Path.Combine(currentPath,"KZData.dll");
			var destinationDllPath = Path.Combine(m_projectFolderPath,Path.GetFileName(sourceDllFilePath));

			File.Copy(sourceDllFilePath,destinationDllPath,true);
		}

		public void CreateProject()
		{
			var projectText = @"<Project Sdk=""Microsoft.NET.Sdk""><PropertyGroup><TargetFramework>netstandard2.1</TargetFramework><AssemblyName>KZProto</AssemblyName></PropertyGroup><ItemGroup><PackageReference Include=""MessagePack"" Version=""3.0.300"" /></ItemGroup><ItemGroup><Reference Include=""KZData""><HintPath>.KZData.dll</HintPath></Reference></ItemGroup></Project>";

			Utility.WriteTextToFile(m_projectFilePath,projectText);
		}

		public void BuildProject()
		{
			var process = new System.Diagnostics.Process();
			process.StartInfo.FileName = "dotnet";
			process.StartInfo.Arguments = $"build \"{m_projectFilePath}\" --configuration Release --output \"{m_outputFolderPath}\"";
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.UseShellExecute = false;

			process.Start();
			process.WaitForExit();

			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();

			Console.WriteLine("Build Output:");
			Console.WriteLine(output);

			if (!string.IsNullOrEmpty(error))
			{
				Console.WriteLine("Build Error:");
				Console.WriteLine(error);
			}
		}

		public void DeleteProject()
		{
			Directory.Delete(m_projectFolderPath,true);
		}
	}
}