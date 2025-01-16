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

		private const int c_type_index = 1;
		private const int c_scheme_index = 0;
		private const int c_invalid_index = -1;

		private static readonly int[] s_proto_index_array = [c_scheme_index,c_type_index];

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
			var stringBuilder = new StringBuilder();
			var excelReader = new ExcelReader(m_enumExcelFilePath);

			foreach(var sheetName in excelReader.SheetNameGroup)
			{
				stringBuilder.Append($"\tpublic enum {sheetName}{Environment.NewLine}");
				stringBuilder.Append($"\t{{{Environment.NewLine}");

				var index = c_invalid_index;

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

				var protoJaggedArray = excelReader.ExtractRowJaggedArray(sheetName,s_proto_index_array);
				var length = protoJaggedArray[c_scheme_index].Length;
				var keyIndex = 0;

				for(int i=0;i<length;i++)
				{
					var member = protoJaggedArray[c_scheme_index][i];

					// remove overlap
					if(member.StartsWith('#') || memberList.Contains(member))
					{
						continue;
					}

					var type = protoJaggedArray[c_type_index][i];

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