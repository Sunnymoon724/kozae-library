using System;
using System.Globalization;
using System.Threading;
using KZConsole.Utilities;

namespace KZConsole
{
	public static class AppRunner
	{
		// Main의 공통 로직만 추출
		public static void Execute(string[] argumentArray,Action<string[]> onPlayProgram)
		{
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				onPlayProgram(argumentArray); 

				KZCommonKit.WriteLog("Program is done",LogType.Info);

				Console.ReadLine();
			}
			catch(Exception exception)
			{
				KZCommonKit.WriteLog($"Exception : {exception}",LogType.Error);

				Environment.Exit(-1);
			}
		}
	}
}