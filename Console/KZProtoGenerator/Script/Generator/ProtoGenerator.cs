using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using KZLib.KZTool;
using MessagePack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KZConsole
{
	public class ProtoGenerator(string branchName,string branchFilePath)
    {
		private const int c_scheme_index = 0;
		private const int c_branch_index = 1;
		private const int c_value_index = 2;

		private readonly Dictionary<string,bool> m_branchStateDict = [];

		public void GenerateAllProto(List<string> protoFilePathList,IEnumerable<string> codeGroup,string outputFolderPath)
		{
			Console.WriteLine("Generate all proto.");

			Console.WriteLine("-Make branch.");

			MakeBranchStateDict();

			Console.WriteLine("-Compile code");
			var data = CompileCode(codeGroup);

			Console.WriteLine("Generate proto.");
			Console.WriteLine("-Convert csv file.");
			GenerateProto(protoFilePathList,Assembly.Load(data),outputFolderPath);
		}

		private void MakeBranchStateDict()
		{
			var excelReader = new ExcelReader(branchFilePath);

			var sheetName = excelReader.FindSheetName(x=>x.Contains("Branch"));
			var schemeArray = excelReader.ExtractRowArray(sheetName,c_scheme_index);

			if(schemeArray.Length == 0 || !schemeArray.Contains(branchName))
			{
				var header = string.Join("/",schemeArray);

				throw new NullReferenceException($"{branchName} is not exist in {header}. [{branchFilePath}]");
			}

			m_branchStateDict.Clear();

			var branchJaggedArray = excelReader.ExtractColumnJaggedArray(sheetName,c_scheme_index,Array.IndexOf(schemeArray,branchName));
			var length = branchJaggedArray[c_scheme_index].Length;

			for(int i=1;i<length;i++)
			{
				var branch = branchJaggedArray[c_scheme_index][i];

				if(string.IsNullOrEmpty(branch))
				{
					continue;
				}

				if(m_branchStateDict.ContainsKey(branch))
				{
					throw new ArgumentException($"{branch} is already exist. [overlap index = {i}]");
				}

				m_branchStateDict.Add(branch,bool.Parse(branchJaggedArray[1][i]));
			}
		}

		private static byte[] CompileCode(IEnumerable<string> codeGroup)
		{
			var syntaxTreeGroup = codeGroup.Where(x => x != null).Select(x => CSharpSyntaxTree.ParseText(x));

			var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			var referenceList = new List<MetadataReference>
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(MessagePackObjectAttribute).Assembly.Location),
				MetadataReference.CreateFromFile(Path.Combine(baseDirectory,"KZData.dll")),
				MetadataReference.CreateFromFile(Path.Combine(baseDirectory,"UnityEngine.dll")),
			};

			referenceList.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)).Select(a => MetadataReference.CreateFromFile(a.Location)));

			var compilation = CSharpCompilation.Create("KZProto",syntaxTreeGroup,referenceList,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			using var memoryStream = new MemoryStream();

			var result = compilation.Emit(memoryStream);

			if(result.Success)
			{
				Console.WriteLine("-Compilation succeeded.");
				Console.WriteLine("-Save dll & pdb.");

				memoryStream.Seek(0,SeekOrigin.Begin);

				return memoryStream.ToArray();
			}
			else
			{
				var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

				foreach(var diagnostic in failures)
				{
					Console.Error.WriteLine(diagnostic.ToString());
				}

				throw new InvalidOperationException("-Compilation failed.");
			}
		}

		private void GenerateProto(List<string> protoFilePathList,Assembly assembly,string outputFolderPath)
		{
			var stringBuilder = new StringBuilder();
			var protoList = new List<object>();
			var numberHashSet = new HashSet<int>();

			var csvFolderPath = Path.Combine(outputFolderPath,"Csv");
			var byteFolderPath = Path.Combine(outputFolderPath,"Proto");

            for(var i=0;i<protoFilePathList.Count;i++)
			{
                var protoFilePath = protoFilePathList[i];
                var fileName = Path.GetFileNameWithoutExtension(protoFilePath);
                var protoName = $"{fileName}Proto";

				if(string.Equals(fileName,"Branch",StringComparison.Ordinal) || string.Equals(fileName,"Enum",StringComparison.Ordinal))
				{
					continue;
				}

				Console.WriteLine($"-Generate {fileName}");

				var excelReader = new ExcelReader(protoFilePath);

				foreach(var sheetName in excelReader.SheetNameGroup)
				{
					if(!sheetName.StartsWith('+'))
					{
						continue;
					}

					var schemeArray = excelReader.ExtractRowArray(sheetName,c_scheme_index) ?? throw new NullReferenceException($"Scheme is not included in {fileName}");

					stringBuilder.Clear();
					stringBuilder.AppendLine(string.Join(",",schemeArray));

					var dataType = assembly.GetType($"KZLib.KZData.{protoName}") ?? throw new InvalidDataException($"Invalid data in {protoName}");
					var branchArray = excelReader.ExtractColumnArray(sheetName,c_branch_index);

					for(var j=c_value_index;j<branchArray.Length;j++)
					{
						var rowArray = excelReader.ExtractRowArray(sheetName,j);

						// skip # or empty
						if(rowArray.Length == 0)
						{
							continue;
						}

						var branch = branchArray[j];

						if(!m_branchStateDict.TryGetValue(branch,out var result))
						{
							throw new SheetConvertException($"{branch} not exist.",protoFilePath,sheetName,j);
						}

						if(!result)
						{
							continue;
						}

						if(!int.TryParse(rowArray[0],out var number))
						{
							throw new SheetConvertException($"{rowArray[0]} is not number.",protoFilePath,sheetName,j);
						}

						if(numberHashSet.Contains(number))
						{
							throw new SheetConvertException($"{number} is already added.",protoFilePath,sheetName,j);
						}

						numberHashSet.Add(number);

						stringBuilder.AppendLine(string.Join(",",rowArray));
						protoList.Add(excelReader.Deserialize(schemeArray,dataType,rowArray,false));
					}
				}

				var csvFilePath = Path.Combine(csvFolderPath,$"{fileName}.csv");

				Utility.WriteTextToFile(csvFilePath,stringBuilder.ToString());

				var bytes = MessagePackSerializer.Serialize(protoList);
				var byteFilePath = Path.Combine(byteFolderPath,$"{fileName}.bytes");

				Utility.WriteBytesToFile(byteFilePath,bytes);

				Console.WriteLine($"-Save {fileName} proto");
			}
		}
	}
}