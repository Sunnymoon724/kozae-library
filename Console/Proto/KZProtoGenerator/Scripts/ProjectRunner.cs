using System;
using System.Diagnostics;
using KZLib.KZUtility;

namespace KZConsole
{
	public class ProjectRunner
	{
		public static void RunProject(string name, string projectPath, string argument)
		{
			if (!FileUtility.IsFileExist(projectPath))
			{
				_WriteLog($"Project not found: {projectPath}",true);

				return;
			}

			var startInfo = new ProcessStartInfo
			{
				FileName = projectPath,
				Arguments = argument,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

			using Process process = new() { StartInfo = startInfo };

			process.OutputDataReceived += _CreateOutputHandler;

			process.ErrorDataReceived += _CreateErrorHandler;

			try
			{
				_WriteLog($"-{name} Run Start",false);

				process.Start();

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				process.WaitForExit();

				process.CancelOutputRead();
				process.CancelErrorRead();
				
				_WriteLog($"-{name} Run End.",false);

				Console.WriteLine();
			}
			catch(Exception exception)
			{
				_WriteLog($"Error executing build: {exception.Message}",true);
			}
		}
		
		private static void _CreateOutputHandler(object sender, DataReceivedEventArgs argument)
		{
			if(argument.Data != null)
			{
				_WriteLog($"{argument.Data}",false);
			}
		}

		private static void _CreateErrorHandler(object sender, DataReceivedEventArgs argument)
		{
			if(argument.Data != null)
			{
				_WriteLog($"-Error: {argument.Data}",true);
			}
		}
		
		private static void _WriteLog(string message,bool isError)
		{
			if(isError)
			{
				var color = Console.ForegroundColor;

				Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine(message);

				Console.ForegroundColor = color;

				return;
			}
			else
			{
				Console.WriteLine(message);
			}
		}
	}
}