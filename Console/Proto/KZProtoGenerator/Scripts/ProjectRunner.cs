using System;
using System.Diagnostics;
using KZConsole.Utilities;

namespace KZConsole
{
	public class ProjectRunner
	{
		public static void RunProject(string name, string projectPath, string argument)
		{
			if (!KZFileKit.IsFileExist(projectPath))
			{
				KZCommonKit.WriteLog($"Warning : Project not found: {projectPath}",LogType.Warning);

				return;
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

			try
			{
				KZCommonKit.WriteLog($"-{name} Run Start",LogType.Info);

				process.Start();

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				process.WaitForExit();

				process.CancelOutputRead();
				process.CancelErrorRead();

				KZCommonKit.WriteLog($"-{name} Run End.",LogType.Info);
			}
			catch(Exception exception)
			{
				KZCommonKit.WriteLog($"Error executing build : {exception.Message}",LogType.Error);
			}
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