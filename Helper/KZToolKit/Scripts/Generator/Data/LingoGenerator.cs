using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using KZHelper.ToolKits;
using KZLib.Utilities;

namespace KZLib.ToolKits
{
	public class LingoGenerator
	{
		public static bool TryConvertToDictionary<TEnum>(string lingoFilePath,out HashSet<TEnum> languageHashSet,out Dictionary<string,Dictionary<string,string[]>> lingoDict) where TEnum : struct
		{
			var keyHashSet = new HashSet<string>();
			languageHashSet = new HashSet<TEnum>();
			lingoDict = new Dictionary<string,Dictionary<string,string[]>>();

			var fileName = KZFileKit.GetOnlyName(lingoFilePath);
			var excelReader = new ExcelReader(lingoFilePath);

			foreach(var sheetName in excelReader.SheetNameGroup)
			{
				var dataDict = new Dictionary<string,string[]>();

				var schemeArray = excelReader.FindSchemeArray(sheetName);
				var rowLength = excelReader.GetRowSize(sheetName);

				// Set language
				for(var i=1;i<schemeArray.Length;i++)
				{
					var scheme = schemeArray[i];

					if(!Enum.TryParse<TEnum>(scheme,true,out var language))
					{
						throw new KZSheetException($"{scheme}(Scheme) is not SystemLanguage.",fileName,sheetName,i);
					}

					languageHashSet.Add(language);
				}

				for(var i=0;i<rowLength;i++)
				{
					var cellArray = excelReader.FindCellArrayInRow(sheetName,i);

					if(cellArray.Length < 1)
					{
						continue;
					}

					var key = cellArray[0];

					if(!string.Equals(key,"Key") && keyHashSet.Contains(key))
					{
						throw new KZSheetException($"{key}(Key) is already added.",fileName,sheetName,i);
					}

					keyHashSet.Add(key);

					var cellSize = cellArray.Length-1;
					var valueArray = new string[cellSize];

					Array.Copy(cellArray,1,valueArray,0,cellSize);

					dataDict.Add(key,valueArray);
				}

				lingoDict.Add(sheetName,dataDict);
			}

			return true;
		}

		public static void GenerateLocaleTemplateExcelFile(string localeFolderPath,string templateName)
		{
			var workbook = new XLWorkbook();
			var workSheet = workbook.AddWorksheet(templateName);

			workSheet.Cell(1,1).InsertData(new string[] { "Key", "Deprecated", "English", "Korean" });
			workSheet.Cell(2,1).InsertData(new string[] { "%이름", "사용 중단됨", "영어", "한국어" });

			KZCommonKit.GenerateExcelFile(localeFolderPath,templateName,workbook);
		}
	}
}