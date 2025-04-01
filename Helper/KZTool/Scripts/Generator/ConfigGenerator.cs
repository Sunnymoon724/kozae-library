using System;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using KZLib.KZUtility;
using YamlDotNet.Serialization;

namespace KZLib.KZTool
{
	public class ConfigGenerator
	{
		private struct ConfigScheme
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public bool IsUsed { get; set; }
			public string Default { get; set; }
			public string Comment { get; set; }
		}

		public static bool TryGenerateConfig(string configFilePath,string outputFolderPath,string templateText,out string result)
		{
			var stringBuilder = new StringBuilder();

			var excelReader = new ExcelReader(configFilePath);
			var fileName = Path.GetFileNameWithoutExtension(configFilePath);

			if(!excelReader.IsExistSheetName(fileName))
			{
				result = $"{fileName} is not exist in {configFilePath}";

				return false;
			}

			foreach(var scheme in excelReader.DeserializeGroup<ConfigScheme>(fileName))
			{
				if(!scheme.IsUsed)
				{
					continue;
				}

				if(string.IsNullOrEmpty(scheme.Default))
				{
					stringBuilder.Append($"\t\tpublic {scheme.Type} {scheme.Name} {{ get; private set; }} // {scheme.Comment}{Environment.NewLine}");
				}
				else
				{
					stringBuilder.Append($"\t\tpublic {scheme.Type} {scheme.Name} {{ get; private set; }} = {scheme.Default}; // {scheme.Comment}{Environment.NewLine}");
				}
			}

			if(stringBuilder.Length <= 0)
			{
				result = "generate failed. config is empty";

				return false;
			}

			stringBuilder.Length -= Environment.NewLine.Length;

			var configCode = stringBuilder.ToString();

			templateText = templateText.Replace("$ClassName",fileName);
			templateText = templateText.Replace("$Properties",configCode);

			var configCodeFilePath = Path.Combine(outputFolderPath,$"{fileName}Config.generated.cs");

			CommonUtility.GenerateTextFile(configCodeFilePath,templateText,out result);

			return true;
		}

		public static void GenerateConfigTemplateFile(string configFolderPath,string templateName,out string result)
		{
			if(!Directory.Exists(configFolderPath))
			{
				throw new NullReferenceException($"{configFolderPath} is not exist");
			}

			// using var package = new ExcelPackage();

			// var worksheet = package.Workbook.Worksheets.Add(templateName);

			// var schemeArray = new string[] { "Name", "Type", "IsUsed", "Default", "Comment" };
			// worksheet.Cells[1, 1, 1, schemeArray.Length].Value = schemeArray;

			// var commentArray = new string[] { "%이름", "타입", "활성화 됨", "기본 값", "주석" };
			// worksheet.Cells[2, 1, 1, schemeArray.Length].Value = commentArray;

			// package.SaveAs(new FileInfo(Path.Combine(configFolderPath,$"{templateName}.xlsx")));

			// result = $"{templateName} is generated";

			var workbook = new XLWorkbook();
			var workSheet = workbook.AddWorksheet(templateName);

			workSheet.Cell(1,1).InsertData(new string[] { "Name", "Type", "IsUsed", "Default", "Comment" });
			workSheet.Cell(2,1).InsertData(new string[] { "%이름", "타입", "활성화 됨", "기본 값", "주석" });

			CommonUtility.GenerateExcelFile(configFolderPath,templateName,workbook,out result);
		}

		public static void GenerateConfigYamlFile<TConfig>(TConfig config,string configFilePath,out string result)
		{
			var serializer = new SerializerBuilder().IncludeNonPublicProperties().WithTypeConverter(new YamlConverter()).Build();

			var yaml = serializer.Serialize(config);

			CommonUtility.GenerateTextFile(configFilePath,yaml,out result);
		}
	}
}