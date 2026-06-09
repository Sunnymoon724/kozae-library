using System;
using System.Diagnostics;
using System.IO;
using KZConsole.Utilities;

namespace KZConsole
{
	public class ProjectRunner
	{
		public static void RunProject(string name, string projectPath, string argument)
		{
			if(!KZFileKit.IsFileExist(projectPath))
			{
				KZCommonKit.WriteLog($"Project not found: {projectPath}",LogType.Error);

				throw new FileNotFoundException("Project not found.",projectPath);
			}

			var startInfo = new ProcessStartInfo
			{
				FileName = projectPath,
				Arguments = argument,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
			};

			using Process process = new() { StartInfo = startInfo };

			process.OutputDataReceived += _CreateOutputHandler;
			process.ErrorDataReceived += _CreateErrorHandler;

			KZCommonKit.WriteLog($"-{name} Run Start",LogType.Info);

			process.Start();

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			process.WaitForExit();

			process.CancelOutputRead();
			process.CancelErrorRead();

			if(process.ExitCode != 0)
			{
				throw new InvalidOperationException($"{name} failed with exit code {process.ExitCode}.");
			}

			KZCommonKit.WriteLog($"-{name} Run End.",LogType.Info);
		}

		private static void _CreateOutputHandler(object sender,DataReceivedEventArgs argument)
		{
			if(argument.Data != null)
			{
				KZCommonKit.WriteLog($"{argument.Data}",LogType.Info);
			}
		}

		private static void _CreateErrorHandler(object sender,DataReceivedEventArgs argument)
		{
			if(argument.Data != null)
			{
				KZCommonKit.WriteLog($"-Error : {argument.Data}",LogType.Error);
			}
		}
	}
}
