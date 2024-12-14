using System.Reflection;
using System.Text;
using KZLib.KZTool;

namespace KZConsole
{
	public class CodeGenerator(List<string> m_protoFilePathList,string m_enumExcelFilePath)
	{
		private static readonly int[] PROTO_INDEX_ARRAY = [Global.PROTO_SCHEME_INDEX,Global.PROTO_TYPE_INDEX];

		private readonly List<string> m_protoCodeList = [];

		public IEnumerable<string> ProtoCodeGroup => m_protoCodeList;

		private struct EnumScheme
		{
			public string Key { get; set; }
			public string Value { get; set; }
			public string Comment { get; set; }
		}

		public void GenerateAllProtoCode(string outputFolderPath)
		{
			m_protoCodeList.Clear();

			Console.WriteLine("Generate all proto code.");

			Console.WriteLine("Generate enum code.");

			GenerateEnumCode(outputFolderPath);

			Console.WriteLine("Generate proto code.");

			GenerateProtoCode(outputFolderPath);
		}

		private void GenerateEnumCode(string outputFolderPath)
		{
			var stringBuilder = new StringBuilder();
			var excelReader = new ExcelReader(m_enumExcelFilePath);

			foreach(var sheetName in excelReader.SheetNameGroup)
			{
				stringBuilder.Append($"\tpublic enum {sheetName}{Environment.NewLine}");
				stringBuilder.Append($"\t{{{Environment.NewLine}");

				var index = Global.INVALID_INDEX;

				foreach(var scheme in excelReader.DeserializeGroup<EnumScheme>(sheetName,true))
				{
					index = int.TryParse(scheme.Value,out var number) ? number : ++index;

					stringBuilder.Append($"\t\t{scheme.Key} = {index}, // {scheme.Comment}{Environment.NewLine}");
				}

				stringBuilder.Append($"\t}}{Environment.NewLine}{Environment.NewLine}");
			}

			if(stringBuilder.Length <= 0)
			{
				return;
			}

			stringBuilder.Length -= 2*Environment.NewLine.Length;

			var enumCode = stringBuilder.ToString();
			var enumTemplate = ReadEmbeddedResource("KZProtoGenerator.Template.EnumTemplate.txt");

			enumTemplate = enumTemplate.Replace("$Enums",enumCode);

			var enumCodeFilePath = Path.Combine(outputFolderPath,$"Enum.cs");

			Utility.WriteTextToFile(enumCodeFilePath,enumTemplate);

			m_protoCodeList.Add(enumTemplate);
		}

		private void GenerateProtoCode(string outputFolderPath)
		{
			var templateText = ReadEmbeddedResource("KZProtoGenerator.Template.ProtoTemplate.txt");
			var stringBuilder = new StringBuilder();
			var memberList = new List<string>();

			foreach(var protoFilePath in m_protoFilePathList)
			{
				if(string.Equals(protoFilePath,m_enumExcelFilePath) || protoFilePath.Contains("Branch"))
				{
					continue;
				}

				stringBuilder.Clear();
				memberList.Clear();
				var excelReader = new ExcelReader(protoFilePath);
				var fileName = Path.GetFileNameWithoutExtension(protoFilePath);
				var sheetName = excelReader.FindSheetName(x=>x.StartsWith('+'));

				var protoJaggedArray = excelReader.ExtractRowJaggedArray(sheetName,PROTO_INDEX_ARRAY);
				var length = protoJaggedArray[Global.PROTO_SCHEME_INDEX].Length;
				var keyIndex = 0;

				for(int i=0;i<length;i++)
				{
					var member = protoJaggedArray[Global.PROTO_SCHEME_INDEX][i];

					// remove overlap
					if(member.StartsWith('#') || memberList.Contains(member))
					{
						continue;
					}

					var type = protoJaggedArray[Global.PROTO_TYPE_INDEX][i];

					stringBuilder.Append($"\t\t[Key({keyIndex++})]{Environment.NewLine}");
					stringBuilder.Append($"\t\tpublic {type} {member} {{ get; set; }}{Environment.NewLine}");

					memberList.Add(member);
				}

				if(stringBuilder.Length <= 0)
				{
					continue;
				}

				stringBuilder.Length -= Environment.NewLine.Length;

				var protoCode = stringBuilder.ToString();

				var protoTemplate = templateText;
				protoTemplate = protoTemplate.Replace("$ClassName",fileName);
				protoTemplate = protoTemplate.Replace("$Properties",protoCode);

				var protoCodeFilePath = Path.Combine(outputFolderPath,$"{fileName}.cs");

				Utility.WriteTextToFile(protoCodeFilePath,protoTemplate);

				m_protoCodeList.Add(protoTemplate);
			}
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