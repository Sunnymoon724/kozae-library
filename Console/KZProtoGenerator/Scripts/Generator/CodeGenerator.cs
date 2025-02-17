using System.Reflection;
using System.Text;
using KZLib.KZTool;

namespace KZConsole
{
	public class CodeGenerator(List<string> m_protoFilePathList,string m_enumExcelFilePath)
	{
		private struct EnumScheme
		{
			public string Key { get; set; }
			public string Value { get; set; }
			public string Comment { get; set; }
		}

		private static readonly string[] s_exception_file_name_array = ["Branch","Enum","Camera","Motion"]; // Motion,Camera are default proto

		private readonly List<string> m_protoCodeList = [];

		public IEnumerable<string> ProtoCodeGroup => m_protoCodeList;

		public void GenerateAllProtoCode(string outputFolderPath)
		{
			m_protoCodeList.Clear();

			Console.WriteLine("Generate all proto code.");
			Console.WriteLine("-Generate enum code.");

			GenerateEnumCode(outputFolderPath);

			Console.WriteLine("-Generate proto code.");

			GenerateProtoCode(outputFolderPath);
		}

		private void GenerateEnumCode(string outputFolderPath)
		{
			var enumBuilder = new StringBuilder();
			var excelReader = new ExcelReader(m_enumExcelFilePath);

			foreach(var sheetName in excelReader.SheetNameGroup)
			{
				enumBuilder.Append($"\tpublic enum {sheetName}{Environment.NewLine}");
				enumBuilder.Append($"\t{{{Environment.NewLine}");

				var index = -1;

				foreach(var scheme in excelReader.DeserializeGroup<EnumScheme>(sheetName))
				{
					index = int.TryParse(scheme.Value,out var number) ? number : ++index;

					enumBuilder.Append($"\t\t{scheme.Key} = {index}, // {scheme.Comment}{Environment.NewLine}");
				}

				enumBuilder.Append($"\t}}{Environment.NewLine}{Environment.NewLine}");
			}

			if(enumBuilder.Length <= 0)
			{
				return;
			}

			enumBuilder.Length -= 2*Environment.NewLine.Length;

			var enumCode = enumBuilder.ToString();
			var enumTemplate = ReadEmbeddedResource("KZProtoGenerator.Templates.EnumTemplate.txt");

			enumTemplate = enumTemplate.Replace("$Enums",enumCode);

			var enumCodeFilePath = Path.Combine(outputFolderPath,$"Enum.cs");

			Utility.WriteTextToFile(enumCodeFilePath,enumTemplate);

			m_protoCodeList.Add(enumTemplate);
		}

		private static bool IsExceptionPath(string protoFilePath)
		{
			var fileName = Path.GetFileNameWithoutExtension(protoFilePath);

			return s_exception_file_name_array.Contains(fileName);
		}

		private void GenerateProtoCode(string outputFolderPath)
		{
			var templateText = ReadEmbeddedResource("KZProtoGenerator.Templates.ProtoTemplate.txt");

			foreach(var protoFilePath in m_protoFilePathList)
			{
				if(IsExceptionPath(protoFilePath))
				{
					continue;
				}

				var excelReader = new ExcelReader(protoFilePath);
				var fileName = Path.GetFileNameWithoutExtension(protoFilePath);
				var sheetNameArray = excelReader.FindSheetNameArray(x=>x.StartsWith('+'));
				var nameCount = sheetNameArray.Length;

				if(nameCount < 1)
				{
					Console.WriteLine($"Warning : {fileName} is not include +Sheet");

					continue;
				}

				var mainClassCode = string.Empty;
				var subClassCode = string.Empty;

				var mainSheetName = Utility.RemovePlusHeader(sheetNameArray[0]);

				mainClassCode = GenerateClassTemplate(excelReader,sheetNameArray[0],$"{mainSheetName}Proto : IProto",protoFilePath);

				if(nameCount != 1)
				{
					var classBuilder = new StringBuilder();

					for(var i=1;i<nameCount;i++)
					{
						var sheetName = Utility.RemovePlusHeader(sheetNameArray[i]);

						classBuilder.Append($"{Environment.NewLine}{Environment.NewLine}{GenerateClassTemplate(excelReader,sheetNameArray[i],$"{sheetName}",protoFilePath)}");
					}

					subClassCode = classBuilder.ToString();
				}

				var protoTemplate = templateText.Replace("$MainClass",mainClassCode);
				protoTemplate = protoTemplate.Replace("$SubClass",subClassCode);

				var protoCodeFilePath = Path.Combine(outputFolderPath,$"{fileName}.cs");

				Utility.WriteTextToFile(protoCodeFilePath,protoTemplate);

				m_protoCodeList.Add(protoTemplate);
			}
		}

		private static string GenerateClassTemplate(ExcelReader excelReader,string sheetName,string className,string filePath)
		{
			var propertyCode = GeneratePropertyCode(excelReader,sheetName);

			if(string.IsNullOrEmpty(propertyCode))
			{
				throw new NullReferenceException($"Generate failed in {sheetName}. [{filePath}]");
			}

			var classBuilder = new StringBuilder();

			classBuilder.Append($"\t[MessagePackObject]{Environment.NewLine}");
			classBuilder.Append($"\tpublic partial class {className}{Environment.NewLine}");
			classBuilder.Append($"\t{{{Environment.NewLine}");
			classBuilder.Append($"{propertyCode}{Environment.NewLine}");
			classBuilder.Append($"\t}}");

			return classBuilder.ToString();
		}

		private static string GeneratePropertyCode(ExcelReader excelReader,string sheetName)
		{
			var propertyList = new List<string>();
			var propertyBuilder = new StringBuilder();
			var protoJaggedArray = excelReader.MergeCellArrayInRows(sheetName,[Global.EXCEL_SCHEME_INDEX,Global.EXCEL_TYPE_INDEX]);
			var schemeArray = protoJaggedArray[Global.EXCEL_SCHEME_INDEX];
			var schemeLength = schemeArray.Length;
			var keyIndex = 0;

			for(int i=0;i<schemeLength;i++)
			{
				var property = schemeArray[i].Split(':')[0];

				// remove overlap
				if(string.IsNullOrEmpty(property) || property.StartsWith('%') || propertyList.Contains(property))
				{
					continue;
				}

				var type = protoJaggedArray[Global.EXCEL_TYPE_INDEX][i];

				propertyBuilder.Append($"\t\t[Key({keyIndex++})]{Environment.NewLine}");
				propertyBuilder.Append($"\t\tpublic {type} {property} {{ get; private set; }}{Environment.NewLine}");

				propertyList.Add(property);
			}

			if(propertyBuilder.Length <= 0)
			{
				return string.Empty;
			}

			propertyBuilder.Length -= Environment.NewLine.Length;

			return propertyBuilder.ToString();
		}

		private static string ReadEmbeddedResource(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using var stream = assembly.GetManifestResourceStream(fileName) ?? throw new FileNotFoundException($"Resource not found. [{fileName}]");
			using var streamReader = new StreamReader(stream);

			return streamReader.ReadToEnd();
		}
	}
}