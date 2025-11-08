using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using KZLib.KZTool;
using KZLib.KZUtility;

namespace KZConsole
{
	public class CodeGenerator
	{
		private struct EnumScheme
		{
			public string Key { get; set; }
			public string Value { get; set; }
			public string Comment { get; set; }
		}

		private static readonly string[] s_exceptionFileNameArray = ["Branch","Enum"];

		private readonly List<string> m_protoCodeList = [];
		private readonly List<string> m_protoFilePathList = [];
		private readonly string m_enumExcelFilePath = string.Empty;
		private readonly Assembly m_assembly = null!;

		public IEnumerable<string> ProtoCodeGroup => m_protoCodeList;

		public CodeGenerator(List<string> protoFilePathList,string enumExcelFilePath)
		{
			m_protoFilePathList = protoFilePathList;
			m_enumExcelFilePath = enumExcelFilePath;

			var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"KZData.dll");

			m_assembly = Assembly.LoadFrom(dllPath);
		}

		public void GenerateAllProtoCode(string outputFolderPath)
		{
			m_protoCodeList.Clear();

			Console.WriteLine("Generate all proto code.");
			Console.WriteLine("-Generate enum code.");

			_GenerateEnumCode(outputFolderPath);

			Console.WriteLine("-Generate proto code.");

			_GenerateProtoCode(outputFolderPath);
		}

		private void _GenerateEnumCode(string outputFolderPath)
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
			var enumTemplate = _ReadEmbeddedResource("KZProtoGenerator.Templates.EnumTemplate.txt");

			enumTemplate = enumTemplate.Replace("$Enums",enumCode);

			var enumCodeFilePath = Path.Combine(outputFolderPath,$"Enum.cs");

			FileUtility.WriteTextToFile(enumCodeFilePath,enumTemplate);

			m_protoCodeList.Add(enumTemplate);
		}

		private void _GenerateProtoCode(string outputFolderPath)
		{
			var templateText = _ReadEmbeddedResource("KZProtoGenerator.Templates.ProtoTemplate.txt");
			var protoCodeBag = new ConcurrentBag<string>();

			Parallel.ForEach(m_protoFilePathList,(protoFilePath)=>
			{
				var fileName = Path.GetFileNameWithoutExtension(protoFilePath);

				if(s_exceptionFileNameArray.Contains(fileName))
				{
					return;
				}

				if(_IsDefaultProtoType($"{fileName}Proto"))
				{
					return;
				}

				var excelReader = new ExcelReader(protoFilePath);
				var sheetNameArray = excelReader.FindSheetNameArray(x=>x.StartsWith('+'));
				var nameCount = sheetNameArray.Length;

				if(nameCount < 1)
				{
					Console.WriteLine($"Warning : {fileName} is not include +Sheet");

					return;
				}

				var mainClassCode = string.Empty;
				var subClassCode = string.Empty;

				var mainSheetName = sheetNameArray[0].TrimStart('+');

				mainClassCode = _GenerateClassTemplate(excelReader,sheetNameArray[0],$"{mainSheetName}Proto : IProto",protoFilePath);

				if(nameCount != 1)
				{
					var classBuilder = new StringBuilder();

					for(var i=1;i<nameCount;i++)
					{
						var sheetName = sheetNameArray[i].TrimStart('+');

						classBuilder.Append($"{Environment.NewLine}{Environment.NewLine}{_GenerateClassTemplate(excelReader,sheetNameArray[i],$"{sheetName}",protoFilePath)}");
					}

					subClassCode = classBuilder.ToString();
				}

				var protoTemplate = templateText.Replace("$MainClass",mainClassCode);
				protoTemplate = protoTemplate.Replace("$SubClass",subClassCode);

				var protoCodeFilePath = Path.Combine(outputFolderPath,$"{fileName}.cs");

				FileUtility.WriteTextToFile(protoCodeFilePath,protoTemplate);

				protoCodeBag.Add(protoTemplate);
			});

			m_protoCodeList.AddRange(protoCodeBag);
		}

		private static string _GenerateClassTemplate(ExcelReader excelReader,string sheetName,string className,string filePath)
		{
			var propertyCode = _GeneratePropertyCode(excelReader,sheetName);

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

		private static string _GeneratePropertyCode(ExcelReader excelReader,string sheetName)
		{
			var propertyList = new List<string>();
			var propertyBuilder = new StringBuilder();
			var protoJaggedArray = excelReader.MergeCellArrayInRows(sheetName,[0,1]);
			var schemeArray = protoJaggedArray[0];
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

				var type = protoJaggedArray[1][i];

				propertyBuilder.Append($"\t\t[Key({keyIndex++})]{Environment.NewLine}");
				propertyBuilder.Append($"\t\tpublic {type} {property} {{ get; init; }}{Environment.NewLine}");

				propertyList.Add(property);
			}

			if(propertyBuilder.Length <= 0)
			{
				return string.Empty;
			}

			propertyBuilder.Length -= Environment.NewLine.Length;

			return propertyBuilder.ToString();
		}

		private static string _ReadEmbeddedResource(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using var stream = assembly.GetManifestResourceStream(fileName) ?? throw new FileNotFoundException($"Resource not found. [{fileName}]");
			using var streamReader = new StreamReader(stream);

			return streamReader.ReadToEnd();
		}
		
		private bool _IsDefaultProtoType(string className)
		{
			var proto = m_assembly.GetType($"KZLib.KZData.{className}");

			return proto != null;
		}
	}
}