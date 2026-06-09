using ClosedXML.Excel;

namespace KZLib.ToolKits
{
	public class ProtoGenerator
	{
		// public static bool TryGenerateConfig(string configFilePath,string outputFolderPath,string templateText,out string result)
		// {
		// 	var stringBuilder = new StringBuilder();

		// 	var excelReader = new ExcelReader(configFilePath);
		// 	var fileName = Path.GetFileNameWithoutExtension(configFilePath);
		// 	var sheetName = excelReader.FindSheetName(x=>string.Equals(x,fileName));

		// 	foreach(var scheme in excelReader.DeserializeGroup<ConfigScheme>(sheetName,true))
		// 	{
		// 		if(!scheme.IsUsed)
		// 		{
		// 			continue;
		// 		}

		// 		stringBuilder.Append($"\t\tpublic {scheme.Type} {scheme.Name} {{ get; init; }} = {scheme.Default}; // {scheme.Comment}{Environment.NewLine}");
		// 	}

		// 	if(stringBuilder.Length <= 0)
		// 	{
		// 		result = "generate failed. config is empty";

		// 		return false;
		// 	}

		// 	stringBuilder.Length -= Environment.NewLine.Length;

		// 	var configCode = stringBuilder.ToString();

		// 	templateText = templateText.Replace("$Properties",configCode);

		// 	var configCodeFilePath = Path.Combine(outputFolderPath,$"{fileName}.generated.cs");

		// 	Directory.CreateDirectory(outputFolderPath);

		// 	File.WriteAllText(configCodeFilePath,templateText,Encoding.UTF8);

		// 	result = "generate success.";

		// 	return true;
		// }

		// public static void GenerateConfigTemplateFile(string configFolderPath,string templateName,out string result)
		// {
		// 	if(!Directory.Exists(configFolderPath))
		// 	{
		// 		throw new NullReferenceException($"{configFolderPath} is not exist");
		// 	}

		// 	var workbook = new XLWorkbook();
		// 	var worksheet = workbook.AddWorksheet(templateName);

		// 	worksheet.Cell(1,1).InsertData(new string[] { "Name", "Type", "IsUsed", "Default", "Comment" });
		// 	worksheet.Cell(2,1).InsertData(new string[] { "%이름", "타입", "활성화 됨", "기본 값", "주석" });

		// 	var filePath = Path.Combine(configFolderPath,$"{templateName}.xlsx");

		// 	workbook.SaveAs(filePath);

		// 	result = $"{templateName} generated";
		// }

		// public static void GenerateConfigYamlFile<TConfig>(TConfig config,string configFilePath,out string result)
		// {
		// 	var serializer = new SerializerBuilder().IncludeNonPublicProperties().WithTypeConverter(new YamlConverter()).Build();

		// 	var yaml = serializer.Serialize(config);

		// 	File.WriteAllText(configFilePath,yaml,Encoding.UTF8);

		// 	result = $"{configFilePath} generated";
		// }

		public static void GenerateMotionProtoFile(string protoFolderPath)
		{
			var workbook = new XLWorkbook();
			var motionSheet = workbook.AddWorksheet("+Motion");

			motionSheet.Cell(1,1).InsertData(new string[] { "Num", "%Branch", "StateName", "EventArray" });
			motionSheet.Cell(2,1).InsertData(new string[] { "int", "string", "string", "MotionEvent[]" });
			motionSheet.Cell(3,1).InsertData(new string[] { "%번호", "브랜치", "애니메이션 이름", "이벤트 배열" });

			var eventSheet = workbook.AddWorksheet("+Event");

			eventSheet.Cell(1,1).InsertData(new string[] { "Order", "EffectPath", "PositionOffset", "StartBone" });
			eventSheet.Cell(2,1).InsertData(new string[] { "int", "string", "Vector3", "string" });
			eventSheet.Cell(3,1).InsertData(new string[] { "%순서", "이펙트 경로", "위치 오프셋", "시작 위치" });

			KZCommonKit.GenerateExcelFile(protoFolderPath,"Motion",workbook);
		}
	}
}