using System.Reflection;
using System.Text;
using KZLib.KZTool;

namespace KZConsole
{
	public class CodeGenerator(List<string> protoFilePathList,string enumFilePath)
	{
		// 0 -> Property / 1 -> Type / 2 -> Comment
		private readonly int[] PROTO_INDEX_ARRAY = [0, 1, 2];

		private struct EnumScheme
		{
			public string Key { get; set; }
			public string Value { get; set; }
			public string Comment { get; set; }
		}

		protected readonly string NewLine = Environment.NewLine;

		public List<string> GenerateAllProtoCode()
		{
			var protoCodeList = new List<string>();

			Console.WriteLine("Generate all proto code.");

			Console.WriteLine("Generate enum code.");

			GenerateEnumCode(ref protoCodeList);

			Console.WriteLine("Generate proto code.");

			GenerateProtoCode(ref protoCodeList);

			return protoCodeList;
		}

		private void GenerateEnumCode(ref List<string> protoCodeList)
		{
			var stringBuilder = new StringBuilder();
			var excelReader = new ExcelReader(enumFilePath);

			foreach(var sheetName in excelReader.SheetNameGroup)
			{
				stringBuilder.Append($"\tpublic enum {sheetName}{NewLine}");
				stringBuilder.Append($"\t{{{NewLine}");

				var index = -1;

				foreach(var scheme in excelReader.Deserialize<EnumScheme>(sheetName))
				{
					index = int.TryParse(scheme.Value,out var number) ? number : ++index;

					stringBuilder.Append($"\t\t{scheme.Key} = {index} // {scheme.Comment}{NewLine}");
				}

				stringBuilder.Append($"\t}}{NewLine}{NewLine}");
			}

			if(stringBuilder.Length <= 0)
			{
				return;
			}

			stringBuilder.Length -= 2*NewLine.Length;

			var enumCode = stringBuilder.ToString();
			var enumTemplate = ReadEmbeddedResource("ProtoForge.Template.EnumTemplate.txt");

			enumTemplate = enumTemplate.Replace("$Enums",enumCode);

			protoCodeList.Add(enumTemplate);
		}

		private void GenerateProtoCode(ref List<string> protoCodeList)
		{
			var templateText = ReadEmbeddedResource("ProtoForge.Template.ProtoDataTemplate.txt");
			var stringBuilder = new StringBuilder();
			var memberList = new List<string>();

			foreach(var protoFilePath in protoFilePathList)
			{
				if(string.Equals(protoFilePath,enumFilePath) || protoFilePath.Contains("Branch"))
				{
					continue;
				}

				//! Generate proto

				stringBuilder.Clear();
				memberList.Clear();
				var excelReader = new ExcelReader(protoFilePath);

				var sheetName = excelReader.FirstSheetName;

				// 0 -> Property / 1 -> Type / 2 -> Comment
				var protoJaggedArray = excelReader.ExtractColumnJaggedArray(sheetName,PROTO_INDEX_ARRAY);
				var length = protoJaggedArray[0].Length;

				for(int i=0;i<length;i++)
				{
					var member = protoJaggedArray[0][i];

					// remove overlap
					if(member.StartsWith('#') || memberList.Contains(member))
					{
						continue;
					}

					var type = protoJaggedArray[1][i];
					var comment = protoJaggedArray[2][i];

					stringBuilder.Append($"\t\tKey({i}){NewLine}");
					stringBuilder.Append($"\t\tpublic {type} {member} {{ get; set; }} //{comment}{NewLine}");

					memberList.Add(member);
				}

				if(stringBuilder.Length <= 0)
				{
					continue;
				}

				stringBuilder.Length -= NewLine.Length;

				var protoCode = stringBuilder.ToString();

				var protoTemplate = templateText;
				protoTemplate = protoTemplate.Replace("$ClassName",sheetName);
				protoTemplate = protoTemplate.Replace("$Properties",protoCode);

				protoCodeList.Add(protoTemplate);
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