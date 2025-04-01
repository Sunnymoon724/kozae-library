using System;
using ClosedXML.Excel;
using System.IO;
using KZLib.KZUtility;

namespace KZLib.KZTool
{
	internal static class CommonUtility
	{
		internal static void GenerateExcelFile(string folderPath,string fileName,XLWorkbook workbook,out string result)
		{
			if(!Directory.Exists(folderPath))
			{
				throw new NullReferenceException($"{folderPath} is not exist");
			}

			var filePath = Path.Combine(folderPath,$"{fileName}.xlsx");

			workbook.SaveAs(filePath);

			result = $"{fileName} is generated";
		}

		internal static void GenerateTextFile(string filePath,string text,out string result)
		{
			FileUtility.WriteTextToFile(filePath,text);

			result = $"{filePath} is generated";
		}
	}
}