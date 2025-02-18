using System.Globalization;
using KZLib.KZTool;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> colorExcelFilePath / 1 -> resultFolderPath
		/// </summary>
		private static void Main(string[] argumentArray)
		{
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				var colorExcelFilePath = argumentArray[0];

				var sheetName = "+Color";
				var excelReader = new ExcelReader(colorExcelFilePath);

				if(!excelReader.IsExistSheetName(sheetName))
				{
					throw new FileNotFoundException($"{colorExcelFilePath} is not ColorProto.");
				}

				Console.WriteLine("Read color proto.");

				var colorReader = new ColorReader();
				var colorListDict = colorReader.ConvertColorListDict(colorExcelFilePath);

				Console.WriteLine("Generate palette.");

				var resultFolderPath = argumentArray[1];

				Directory.CreateDirectory(resultFolderPath);

				var paletteGenerator = new PaletteGenerator();

				foreach(var pair in colorListDict)
				{
					Console.WriteLine($"-Generate {pair.Key}.");

					var imagePath = Path.Combine(resultFolderPath,$"{pair.Key}.png");

					paletteGenerator.GeneratorPaletteImage(pair.Value,imagePath);

					Console.WriteLine($"{pair.Key} is done.");
				}
			}
			catch(Exception exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{exception}");
				Console.ResetColor();

				Environment.Exit(-1);
			}
		}
	}
}