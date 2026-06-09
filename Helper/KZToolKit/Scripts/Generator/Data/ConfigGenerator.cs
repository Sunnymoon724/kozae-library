using System;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using KZLib.Utilities;
using YamlDotNet.Serialization;

namespace KZLib.ToolKits
{
	public class ConfigGenerator
	{
		private struct ConfigScheme
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public bool Deprecated { get; set; }
			public string Default { get; set; }
			public string Comment { get; set; }
		}

		public static void GenerateConfig(string configFilePath,string outputFolderPath,string templateText)
		{
			var stringBuilder = new StringBuilder();

			var excelReader = new ExcelReader(configFilePath);
			var fileName = KZFileKit.GetOnlyName(configFilePath);

			if(!excelReader.IsExistSheetName(fileName))
			{
				throw new FileNotFoundException($"{fileName}(sheet) is not exist in {configFilePath}");
			}

			try
			{
				foreach(var scheme in excelReader.DeserializeGroup<ConfigScheme>(fileName))
				{
					if(scheme.Deprecated)
					{
						continue;
					}

					var defaultText = string.IsNullOrEmpty(scheme.Comment) ? $"" : $" = {scheme.Default};";
					var commentText = string.IsNullOrEmpty(scheme.Comment) ? $"{Environment.NewLine}" : $" // {scheme.Comment}{Environment.NewLine}";

					stringBuilder.Append($"\t\tpublic {scheme.Type} {scheme.Name} {{ get; private set; }}{defaultText}{commentText}");
				}
			}
			catch(Exception exception)
			{
				throw new KZSheetException($"{exception.Message}",configFilePath,fileName,-1);
			}

			

			if(stringBuilder.Length <= 0)
			{
				throw new InvalidDataException("Generate failed. config is empty");
			}

			stringBuilder.Length -= Environment.NewLine.Length;

			var configCode = stringBuilder.ToString();

			templateText = templateText.Replace("$ClassName",fileName);
			templateText = templateText.Replace("$Properties",configCode);

			var configCodeFilePath = Path.Combine(outputFolderPath,$"{fileName}Config.generated.cs");

			KZCommonKit.GenerateTextFile(configCodeFilePath,templateText);
		}

		public static void GenerateConfigTemplateExcelFile(string configFolderPath,string templateName)
		{
			var workbook = new XLWorkbook();
			var workSheet = workbook.AddWorksheet(templateName);

			workSheet.Cell(1,1).InsertData(new string[] { "Name", "Type", "Deprecated", "Default", "Comment" });
			workSheet.Cell(2,1).InsertData(new string[] { "%이름", "타입", "사용 중단됨", "기본 값", "주석" });

			KZCommonKit.GenerateExcelFile(configFolderPath,templateName,workbook);
		}

		public static void GenerateConfigYamlFile<TConfig>(TConfig config,string configFilePath)
		{
			var serializer = new SerializerBuilder().IncludeNonPublicProperties().WithTypeConverter(new YamlConverter()).Build();

			var yaml = serializer.Serialize(config);

			KZCommonKit.GenerateTextFile(configFilePath,yaml);
		}
	}
}