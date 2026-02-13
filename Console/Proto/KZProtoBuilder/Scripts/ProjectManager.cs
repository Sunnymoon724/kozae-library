using System;
using System.IO;
using System.Reflection;
using KZConsole.Utilities;
using KZLib.Utilities;

namespace KZConsole
{
	public class ProjectManager
	{
		private readonly string m_projectFolderPath = string.Empty;

		public ProjectManager(string projectFolderPath)
		{
			m_projectFolderPath = projectFolderPath;

			FileUtility.CreateFolder(m_projectFolderPath);
		}

		public void CreateProject()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();

			_CreateProjectFile(assembly);
			_CreateGlobalFile(assembly);

			_CopyPluginFile();
		}

		private void _CreateProjectFile(Assembly assembly)
		{
			var projectFileDict = CommonUtility.ReadEmbeddedResourcesFromExtension(assembly,".txt");

			if(!projectFileDict.TryGetValue("ProtoProject.txt", out var projectFile))
			{
				throw new FileNotFoundException("Project file not found.");
			}

			_WriteTextToFile("ProtoProject.csproj",projectFile);
		}

		private void _CreateGlobalFile(Assembly assembly)
		{
			var codeFileDict = CommonUtility.ReadEmbeddedResourcesFromExtension(assembly,".cs");

			if(!codeFileDict.TryGetValue("Global.cs", out var globalFile))
			{
				throw new FileNotFoundException("Project file not found.");
			}

			_WriteTextToFile("Global.cs",globalFile);
		}

		private void _CopyPluginFile()
		{
			var dllFilePath = Path.Combine(FileUtility.GetProjectPath(),"KZData.dll");

			FileUtility.CopyFile(dllFilePath,m_projectFolderPath,true);
		}

		private void _WriteTextToFile(string fileName,string text)
		{
			var filePath = Path.Combine(m_projectFolderPath,fileName);

			FileUtility.WriteTextToFile(filePath,text);
		}

		public void BuildProject()
		{
			var projectFilePath = Path.Combine(m_projectFolderPath,"ProtoProject.csproj");

			var process = new System.Diagnostics.Process();
			process.StartInfo.FileName = "dotnet";
			process.StartInfo.Arguments = $"build \"{projectFilePath}\" --configuration Release";
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.UseShellExecute = false;

			CommonUtility.WriteLog("-Build start",LogType.Info);

			try
			{
				process.Start();

				var output = process.StandardOutput.ReadToEnd();
				var error = process.StandardError.ReadToEnd();

				process.WaitForExit();

				CommonUtility.WriteLog($"-Build Output: \n{output}",LogType.Info);

				if(!string.IsNullOrEmpty(error))
				{
					CommonUtility.WriteLog($"-Build Error: \n{error}",LogType.Error);
				}
			}
			catch(Exception exception)
			{
				CommonUtility.WriteLog($"Error executing build: {exception.Message}",LogType.Error);
			}
		}
	}
}