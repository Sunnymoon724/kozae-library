using System;
using System.Globalization;
using System.Threading;
using KZConsole.Utilities;

namespace KZConsole
{
	public static class AppRunner
	{
		private const string c_skipPauseEnvironmentVariable = "SKIP_PAUSE";

		// Main의 공통 로직만 추출
		public static void Execute(string[] argumentArray,int requiredCount,string usage,Action<string[]> onPlayProgram)
		{
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				if(argumentArray == null || argumentArray.Length < requiredCount)
				{
					KZCommonKit.WriteLog($"Invalid arguments. Usage: {usage}",LogType.Error);

					Environment.Exit(-1);

					return;
				}

				onPlayProgram(argumentArray);

				KZCommonKit.WriteLog("Program is done",LogType.Info);

				_WaitForExitIfNeeded();
			}
			catch(Exception exception)
			{
				KZCommonKit.WriteLog($"Exception : {exception}",LogType.Error);

				Environment.Exit(-1);
			}
		}

		private static void _WaitForExitIfNeeded()
		{
			if(!_ShouldWaitForExit())
			{
				return;
			}

			KZCommonKit.WriteLog("Press Enter to exit...",LogType.Info);

			Console.ReadLine();
		}

		private static bool _ShouldWaitForExit()
		{
			var skipPause = Environment.GetEnvironmentVariable(c_skipPauseEnvironmentVariable);

			if(skipPause == "1" || string.Equals(skipPause,"true",StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if(Console.IsInputRedirected)
			{
				return false;
			}

			if(!Environment.UserInteractive)
			{
				return false;
			}

			return true;
		}
	}
}
