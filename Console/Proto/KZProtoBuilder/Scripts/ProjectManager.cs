using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using KZConsole.Utilities;

namespace KZConsole
{
	public class ProjectManager
	{
		private readonly string m_projectFolderPath = string.Empty;

		public ProjectManager(string projectFolderPath)
		{
			m_projectFolderPath = projectFolderPath;

			KZFileKit.CreateFolder(m_projectFolderPath);
		}

		public void CreateProject()
		{
			var assembly = Assembly.GetExecutingAssembly();

			_CreateProjectFile(assembly);

			_CopyPluginFile();
		}

		private void _CreateProjectFile(Assembly assembly)
		{
			var projectFileDict = KZCommonKit.ReadEmbeddedResourcesFromExtension(assembly,".txt");

			if(!projectFileDict.TryGetValue("ProtoProject.txt", out var projectFile))
			{
				throw new FileNotFoundException("Project file not found.");
			}

			KZFileKit.WriteTextToFile(m_projectFolderPath,"ProtoProject.csproj",projectFile);
		}

		private void _CopyPluginFile()
		{
			var dllFilePath = Path.Combine(KZFileKit.GetProjectPath(),"KZData.dll");

			KZFileKit.CopyFile(dllFilePath,m_projectFolderPath,true);
		}

		public void BuildProject()
		{
			var projectFilePath = Path.Combine(m_projectFolderPath,"ProtoProject.csproj");

			using var process = new Process();

			process.StartInfo.FileName = "dotnet";
			process.StartInfo.Arguments = $"build \"{projectFilePath}\" --configuration Release";
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.UseShellExecute = false;

			KZCommonKit.WriteLog("-Build start",LogType.Info);

			process.Start();

			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();

			process.WaitForExit();

			KZCommonKit.WriteLog($"-Build Output: \n{output}",LogType.Info);

			if(!string.IsNullOrEmpty(error))
			{
				KZCommonKit.WriteLog($"-Build Error: \n{error}",LogType.Error);
			}

			if(process.ExitCode != 0)
			{
				throw new InvalidOperationException($"Build failed with exit code {process.ExitCode}.");
			}
		}
	}
}