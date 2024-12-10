using System.Reflection;
using System.Text;
using KZLib.KZTool;

namespace KZConsole
{
	public class CodeGenerator(List<string> protoFilePathList,string enumFilePath)
	{
		private readonly int[] PROTO_INDEX_ARRAY = [Global.PROTO_SCHEME_INDEX,Global.PROTO_TYPE_INDEX];

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

				var index = Global.INVALID_INDEX;

				foreach(var scheme in excelReader.DeserializeGroup<EnumScheme>(sheetName,true))
				{
					index = int.TryParse(scheme.Value,out var number) ? number : ++index;

					stringBuilder.Append($"\t\t{scheme.Key} = {index}, // {scheme.Comment}{NewLine}");
				}

				stringBuilder.Append($"\t}}{NewLine}{NewLine}");
			}

			if(stringBuilder.Length <= 0)
			{
				return;
			}

			stringBuilder.Length -= 2*NewLine.Length;

			var enumCode = stringBuilder.ToString();
			var enumTemplate = ReadEmbeddedResource("KZProtoGenerator.Template.EnumTemplate.txt");

			enumTemplate = enumTemplate.Replace("$Enums",enumCode);

			protoCodeList.Add(enumTemplate);
		}

		private void GenerateProtoCode(ref List<string> protoCodeList)
		{
			var templateText = ReadEmbeddedResource("KZProtoGenerator.Template.ProtoTemplate.txt");
			var stringBuilder = new StringBuilder();
			var memberList = new List<string>();

			foreach(var protoFilePath in protoFilePathList)
			{
				if(string.Equals(protoFilePath,enumFilePath) || protoFilePath.Contains("Branch"))
				{
					continue;
				}

				stringBuilder.Clear();
				memberList.Clear();
				var excelReader = new ExcelReader(protoFilePath);

				var protoJaggedArray = excelReader.ExtractRowJaggedArray(excelReader.FirstSheetName,PROTO_INDEX_ARRAY);
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

					stringBuilder.Append($"\t\t[Key({keyIndex++})]{NewLine}");
					stringBuilder.Append($"\t\tpublic {type} {member} {{ get; set; }}{NewLine}");

					memberList.Add(member);
				}

				if(stringBuilder.Length <= 0)
				{
					continue;
				}

				stringBuilder.Length -= NewLine.Length;

				var protoCode = stringBuilder.ToString();

				var protoTemplate = templateText;
				protoTemplate = protoTemplate.Replace("$ClassName",Path.GetFileNameWithoutExtension(protoFilePath));
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