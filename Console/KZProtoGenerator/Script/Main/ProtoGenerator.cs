using System.Reflection;
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

		private string _csvFolderPath = "";
		private string _byteFolderPath = "";

		public void GenerateAllProto(List<string> protoFilePathList,List<string> codeList,string outputFolderPath)
		{
			Console.WriteLine("Generate all proto.");

			Console.WriteLine("Make branch.");

			MakeBranchStateDict();

			Console.WriteLine("Convert csv file.");

			_csvFolderPath = Path.Combine(outputFolderPath,"Csv");
			_byteFolderPath = Path.Combine(outputFolderPath,"Proto");

			var data = CompileCode(codeList);

			CreateData(protoFilePathList,Assembly.Load(data));

			var dllFilePath = Path.Combine(outputFolderPath,"KZProto.dll");

			WriteBytesToFile(dllFilePath,data);
		}

		private void MakeBranchStateDict()
		{
			var excelReader = new ExcelReader(branchFilePath);

			var sheetName = excelReader.FirstSheetName;
			var headerArray = excelReader.ExtractRowArray(sheetName,0);

			if(headerArray.Length == 0 || headerArray.Contains(branchName))
			{
				throw new NullReferenceException($"{branchName} is not exist in {branchFilePath}.");
			}

			_branchStateDict.Clear();

			var branchJaggedArray = excelReader.ExtractRowJaggedArray(sheetName,0,Array.IndexOf(headerArray,branchName));
			var length = branchJaggedArray[0].Length;

			for(int i=0;i<length;i++)
			{
				var branch = branchJaggedArray[0][i];

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

		private void CreateData(List<string> protoFilePathList,Assembly assembly)
		{
			var stringBuilder = new StringBuilder();
			var protoList = new List<object>();

            for(var i=0;i<protoFilePathList.Count;i++)
			{
                var protoFilePath = protoFilePathList[i];
                var protoName = Path.GetFileName(protoFilePath);
				var excelReader = new ExcelReader(protoFilePath);

				var sheetName = excelReader.FirstSheetName;
				var branchArray = excelReader.ExtractColumnArray(sheetName,1);

				stringBuilder.Clear();

				var schemeArray = excelReader.ExtractRowArray(sheetName,0) ?? throw new NullReferenceException($"Scheme is not included in {protoName}");

				// scheme
				stringBuilder.AppendLine(string.Join(",",schemeArray));

				for(var j=0;j<branchArray.Length;j++)
				{
					if(!string.Equals(branchName,branchArray[j],StringComparison.Ordinal))
					{
						continue;
					}

					var dataType = assembly.GetType(protoName) ?? throw new InvalidDataException($"Invalid data in {protoName}");
					var rowArray = excelReader.ExtractRowArray(sheetName,j);

					stringBuilder.AppendLine(string.Join(",",rowArray));
					protoList.Add(excelReader.Deserialize(sheetName,schemeArray,dataType,rowArray));
				}

				var csvFilePath = Path.Combine(_csvFolderPath,$"{protoName}.csv");

				WriteTextToFile(csvFilePath,stringBuilder.ToString());

				var bytes = MessagePackSerializer.Serialize(protoList);
				var byteFilePath = Path.Combine(_byteFolderPath,$"{protoName}.bytes");

				WriteBytesToFile(byteFilePath,bytes);
			}
		}

		private static byte[] CompileCode(List<string> codeList)
		{
			var syntaxTreeGroup = codeList.Select(x => CSharpSyntaxTree.ParseText(x));

			var compilation = CSharpCompilation.Create("DynamicAssembly",syntaxTreeGroup,[MetadataReference.CreateFromFile(typeof(object).Assembly.Location),MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)],new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			using var memoryStream = new MemoryStream();
			var result = compilation.Emit(memoryStream);

			if(!result.Success)
			{
				var failures = result.Diagnostics.Where(diagnostic =>diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

				foreach(var diagnostic in failures)
				{
					Console.Error.WriteLine(diagnostic.ToString());
				}

				throw new InvalidOperationException("Compilation failed.");
			}

			memoryStream.Seek(0,SeekOrigin.Begin);

			return memoryStream.ToArray();
		}

		/// <summary>
		/// Create folder. (path is file ? create parent folder. : create folder)
		/// </summary>
		private static void CreateFolder(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				throw new NullReferenceException("Path is null");
			}

			// Path is file ? Get parent path. : Get path
			var folderPath = (Path.HasExtension(path) ? Path.GetDirectoryName(path) : path) ?? throw new NullReferenceException($"Parent path not exist. [{path}]");

			Directory.CreateDirectory(folderPath);
		}

		private static void WriteTextToFile(string filePath,string text)
		{
			CreateFolder(filePath);

			File.WriteAllText(filePath,text);
		}

		private static void WriteBytesToFile(string filePath,byte[] bytes)
		{
			CreateFolder(filePath);

			File.WriteAllBytes(filePath,bytes);
		}
	}
}