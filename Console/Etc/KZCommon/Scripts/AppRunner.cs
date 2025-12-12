using System;
using System.Globalization;
using System.Threading;

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

				Console.WriteLine("Program is done");

				Console.ReadLine();
			}
			catch(Exception exception)
			{
				var color = Console.ForegroundColor;

				Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine($"{exception}");

				Console.ForegroundColor = color;

				Environment.Exit(-1);
			}
		}
	}
}