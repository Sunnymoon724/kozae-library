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
		private readonly Dictionary<string,bool> _branchStateDict = [];

		public void GenerateAllProto(List<string> protoFilePathList,IEnumerable<string> codeGroup,string outputFolderPath)
		{
			Console.WriteLine("Generate all proto.");

			Console.WriteLine("Make branch.");

			MakeBranchStateDict();

			Console.WriteLine("Compile code");
			var data = CompileCode(codeGroup);

			Console.WriteLine("Convert csv file.");
			CreateData(protoFilePathList,Assembly.Load(data),outputFolderPath);
		}

		private void MakeBranchStateDict()
		{
			var excelReader = new ExcelReader(branchFilePath);

			var sheetName = excelReader.FindSheetName(x=>x.Contains("Branch"));
			var schemeArray = excelReader.ExtractRowArray(sheetName,Global.PROTO_SCHEME_INDEX);

			if(schemeArray.Length == 0 || !schemeArray.Contains(branchName))
			{
				var header = string.Join("/",schemeArray);

				throw new NullReferenceException($"{branchName} is not exist in {header}. [{branchFilePath}]");
			}

			_branchStateDict.Clear();

			var branchJaggedArray = excelReader.ExtractColumnJaggedArray(sheetName,Global.PROTO_SCHEME_INDEX,Array.IndexOf(schemeArray,branchName));
			var length = branchJaggedArray[Global.PROTO_SCHEME_INDEX].Length;

			for(int i=1;i<length;i++)
			{
				var branch = branchJaggedArray[Global.PROTO_SCHEME_INDEX][i];

				if(string.IsNullOrEmpty(branch))
				{
					continue;
				}

				if(_branchStateDict.ContainsKey(branch))
				{
					throw new ArgumentException($"{branch} is already exist. [overlap index = {i}]");
				}

				_branchStateDict.Add(branch,bool.Parse(branchJaggedArray[1][i]));
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
				Console.WriteLine("Compilation succeeded.");
				Console.WriteLine("Save dll & pdb.");

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

				throw new InvalidOperationException("Compilation failed.");
			}
		}

		private void CreateData(List<string> protoFilePathList,Assembly assembly,string outputFolderPath)
		{
			var stringBuilder = new StringBuilder();
			var protoList = new List<object>();

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

				Console.WriteLine($"Create {fileName}");

				var excelReader = new ExcelReader(protoFilePath);

				foreach(var sheetName in excelReader.SheetNameGroup)
				{
					if(!sheetName.StartsWith('+'))
					{
						continue;
					}

					var schemeArray = excelReader.ExtractRowArray(sheetName,Global.PROTO_SCHEME_INDEX) ?? throw new NullReferenceException($"Scheme is not included in {fileName}");

					stringBuilder.Clear();
					stringBuilder.AppendLine(string.Join(",",schemeArray));

					var dataType = assembly.GetType($"KZLib.KZData.{protoName}") ?? throw new InvalidDataException($"Invalid data in {protoName}");
					var branchArray = excelReader.ExtractColumnArray(sheetName,Global.PROTO_BRANCH_INDEX);

					for(var j=Global.PROTO_DATA_INDEX;j<branchArray.Length;j++)
					{
						var rowArray = excelReader.ExtractRowArray(sheetName,j);

						// skip # or empty
						if(rowArray.Length == 0)
						{
							continue;
						}

						var branch = branchArray[j];

						if(!_branchStateDict.TryGetValue(branch,out var result))
						{
							throw new Exception($"{branch} not exist. [file{protoFilePath}/line:{j}]");
						}

						if(!result)
						{
							continue;
						}

						stringBuilder.AppendLine(string.Join(",",rowArray));
						protoList.Add(excelReader.Deserialize(schemeArray,dataType,rowArray,false));
					}
				}

				var csvFilePath = Path.Combine(csvFolderPath,$"{fileName}.csv");

				Utility.WriteTextToFile(csvFilePath,stringBuilder.ToString());

				var bytes = MessagePackSerializer.Serialize(protoList);
				var byteFilePath = Path.Combine(byteFolderPath,$"{fileName}.bytes");

				Utility.WriteBytesToFile(byteFilePath,bytes);
			}
		}
	}
}