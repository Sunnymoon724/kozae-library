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
			public object Default { get; set; }
			public string Comment { get; set; }
		}

		public static bool TryGenerateConfig(string configFilePath,string outputFolderPath,string templateText,out string result)
		{
			var stringBuilder = new StringBuilder();

			var excelReader = new ExcelReader(configFilePath);
			var fileName = Path.GetFileNameWithoutExtension(configFilePath);
			var sheetName = excelReader.FindSheetName(x=>string.Equals(x,fileName));

			foreach(var scheme in excelReader.DeserializeGroup<ConfigScheme>(sheetName,true))
			{
				if(!scheme.IsUsed)
				{
					continue;
				}

				stringBuilder.Append($"\t\tpublic {scheme.Type} {scheme.Name} {{ get; private set; }} = {scheme.Default}; // {scheme.Comment}{Environment.NewLine}");
			}

			if(stringBuilder.Length <= 0)
			{
				result = "generate failed. config is empty";

				return false;
			}

			stringBuilder.Length -= Environment.NewLine.Length;

			var configCode = stringBuilder.ToString();

			templateText = templateText.Replace("$Properties",configCode);

			var configCodeFilePath = Path.Combine(outputFolderPath,$"{fileName}.generated.cs");

			Directory.CreateDirectory(outputFolderPath);

			File.WriteAllText(configCodeFilePath,templateText,Encoding.UTF8);

			result = "generate success.";

			return true;
		}

		public static void GenerateConfigTemplateFile(string configFolderPath,string templateName,out string result)
		{
			if(!Directory.Exists(configFolderPath))
			{
				throw new NullReferenceException($"{configFolderPath} is not exist");
			}

			var workbook = new XLWorkbook();
			var worksheet = workbook.AddWorksheet(templateName);

			worksheet.Cell(1,1).InsertData(new string[] { "Name", "Type", "IsUsed", "Default", "Comment" });
			worksheet.Cell(2,1).InsertData(new string[] { "#이름", "타입", "활성화 됨", "기본 값", "주석" });

			var filePath = Path.Combine(configFolderPath,$"{templateName}.xlsx");

			workbook.SaveAs(filePath);

			result = $"{templateName} generated";
		}

		public static void GenerateConfigYamlFile<TConfig>(TConfig config,string configFilePath,out string result)
		{
			var serializer = new SerializerBuilder().WithTypeConverter(new YamlConverter()).Build();

			var yaml = serializer.Serialize(config);

			File.WriteAllText(configFilePath,yaml,Encoding.UTF8);

			result = $"{configFilePath} generated";
		}
	}
}