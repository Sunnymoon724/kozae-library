using System;
using System.Diagnostics;
using KZConsole.KZUtility;
using KZLib.KZUtility;

namespace KZConsole
{
	public class ProjectRunner
	{
		public static void RunProject(string name, string projectPath, string argument)
		{
			if (!FileUtility.IsFileExist(projectPath))
			{
				CommonUtility.WriteLog($"Warning : Project not found: {projectPath}",LogType.Warning);

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
				CommonUtility.WriteLog($"-{name} Run Start",LogType.Info);

				process.Start();

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				process.WaitForExit();

				process.CancelOutputRead();
				process.CancelErrorRead();

				CommonUtility.WriteLog($"-{name} Run End.",LogType.Info);
			}
			catch(Exception exception)
			{
				CommonUtility.WriteLog($"Error executing build : {exception.Message}",LogType.Error);
			}
		}
		
		private static void _CreateOutputHandler(object sender, DataReceivedEventArgs argument)
		{
			if(argument.Data != null)
			{
				CommonUtility.WriteLog($"{argument.Data}",LogType.Info);
			}
		}

		private static void _CreateErrorHandler(object sender, DataReceivedEventArgs argument)
		{
			if(argument.Data != null)
			{
				CommonUtility.WriteLog($"-Error : {argument.Data}",LogType.Error);
			}
		}
	}
}