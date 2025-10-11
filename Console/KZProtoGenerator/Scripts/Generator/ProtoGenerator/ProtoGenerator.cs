using System.Reflection;
using System.Text;
using KZConsole.KZProto;
using KZLib.KZTool;
using KZLib.KZUtility;
using MessagePack;
using Newtonsoft.Json;

namespace KZConsole
{
	public class ProtoGenerator
	{
		private const string c_branchTag = "%Branch";
		private const int c_valueIndex = 3;
		private const char c_sheetMark = '+';

		private static readonly string[] s_exceptionFileNameArray = ["Branch","Enum"];

		private readonly BranchSheet m_branchSheet = null!;

		private Assembly m_protoAssembly = null!;
		private readonly Assembly m_assembly = null!;

		public ProtoGenerator(string branchName,string branchFilePath)
		{
			m_branchSheet = new(branchName,branchFilePath);

			var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"KZData.dll");

			m_assembly = Assembly.LoadFrom(dllPath);
		}

		public void GenerateAllProto(List<string> protoFilePathList,IEnumerable<string> codeGroup,string outputFolderPath)
		{
			Console.WriteLine("Generate all proto.");

			_GenerateBranch();
			_CompileCode(codeGroup);
			_GenerateProtoFiles(protoFilePathList,outputFolderPath);
		}

		private void _GenerateBranch()
		{
			Console.WriteLine("-Make branch.");

			m_branchSheet.GenerateBranch();
		}

		private void _CompileCode(IEnumerable<string> codeGroup)
		{
			Console.WriteLine("-Compile code");

			m_protoAssembly = CodeCompiler.CompileToAssembly(codeGroup);
		}

		private void _GenerateProtoFiles(List<string> protoFilePathList,string outputFolderPath)
		{
			Console.WriteLine("Generate proto.");

			var csvFolderPath = Path.Combine(outputFolderPath,"Csv");
			var byteFolderPath = Path.Combine(outputFolderPath,"Proto");

			foreach(var protoFilePath in protoFilePathList)
			{
				if(_ShouldSkipFile(protoFilePath))
				{
					continue;
				}

				_ProcessProtoFile(protoFilePath,csvFolderPath,byteFolderPath);
			}
		}

		private static bool _ShouldSkipFile(string protoFilePath)
		{
			var fileName = Path.GetFileNameWithoutExtension(protoFilePath);

			return s_exceptionFileNameArray.Contains(fileName);
		}

		private void _ProcessProtoFile(string protoFilePath,string csvFolderPath,string byteFolderPath)
		{
			var fileName = Path.GetFileNameWithoutExtension(protoFilePath);
			var excelReader = new ExcelReader(protoFilePath);
			
			var protoType = _GetProtoType($"{fileName}Proto");
			var protoArray = _GenerateProtoArray(fileName,protoType,excelReader,csvFolderPath);

			if(protoArray.Length > 0)
			{
				_SaveProto(fileName,protoArray,byteFolderPath);

				Console.WriteLine($"-Save {fileName} proto");
			}
		}

		private static void _SaveProto(string fileName,Array protoArray,string byteFolderPath)
		{
			var arrayType = protoArray.GetType();
			var options = MessagePackSerializerOptions.Standard.WithResolver(MessagePackResolver.Instance);

			var serialized = MessagePackSerializer.Serialize(arrayType,protoArray,options);

			var filePath = Path.Combine(byteFolderPath,$"{fileName}.bytes");

			FileUtility.WriteByteToFile(filePath,serialized);
		}

		private Array _GenerateProtoArray(string fileName,Type protoType,ExcelReader excelReader,string csvFolderPath)
		{
			Console.WriteLine($"-Generate {fileName}");

			// Generate is only +sheet
			var sheetNameArray = excelReader.FindSheetNameArray(x => x.StartsWith(c_sheetMark));

			if(sheetNameArray.Length < 1)
			{
				Console.WriteLine($"Warning : {fileName} is not include +Sheet");

				// Return empty array
				return Array.CreateInstance(protoType,0);
			}

			// first sheet is main class (others are field class)
			var mainSheetName = sheetNameArray[0];

			// Generate field class (support main class)
			var schemeIndexListDict = _GenerateSubSheet(excelReader,sheetNameArray);

			var rowArray = _GenerateRowArray(excelReader,mainSheetName,schemeIndexListDict);
			var protoArray = _ConvertProtoArray(excelReader,rowArray,mainSheetName,protoType,out var content);

			_SaveCsvFile(csvFolderPath,fileName,content);

			return protoArray;
		}

		private Dictionary<Type,List<int>> _GenerateSubSheet(ExcelReader excelReader,string[] sheetNameArray)
		{
			if(sheetNameArray.Length == 1)
			{
				return [];
			}

			var indexListDict = new Dictionary<Type,List<int>>();
			var typeNameArray = excelReader.FindCellArrayInRow(sheetNameArray[0],1);

			for(var i=1;i<sheetNameArray.Length;i++)
			{
				var className = sheetNameArray[i].TrimStart(c_sheetMark);

				_ProcessSubSheetTypes(typeNameArray,className,ref indexListDict);
			}

			return indexListDict;
		}

		private void _ProcessSubSheetTypes(string[] typeNameArray,string className,ref Dictionary<Type,List<int>> schemeIndexListDict)
		{
			for(var j=0;j<typeNameArray.Length;j++)
			{
				var typeName = typeNameArray[j].Replace("[]","");

				if(string.Equals(typeName,className))
				{
					var subClassType = _GetProtoType(className);

					if(!schemeIndexListDict.TryGetValue(subClassType,out List<int>? value))
					{
						value = [];
						schemeIndexListDict[subClassType] = value;
					}

					value.Add(j);
				}
			}
		}

		private static Array _ConvertProtoArray(ExcelReader excelReader,string[][] rowArray,string sheetName,Type protoType,out string content)
		{
			var protoArray = Array.CreateInstance(protoType,rowArray.Length);
			var schemeArray = excelReader.FindSchemeArray(sheetName);

			var builder = new StringBuilder();
			builder.AppendLine(string.Join(",",schemeArray));

			for(var i=0;i<rowArray.Length;i++)
			{
				var escapedRow = rowArray[i].Select(_EscapeText);

				builder.AppendLine(string.Join(",",escapedRow));

				var proto = excelReader.Deserialize(schemeArray,protoType,rowArray[i],i);

				protoArray.SetValue(proto,i);
			}

			content = builder.ToString();

			return protoArray;
		}

		private static string _EscapeText(string content)
		{
			if(string.IsNullOrEmpty(content))
			{
				return string.Empty;
			}

			bool mustQuote = content.Contains(',') || content.Contains('"') || content.Contains('\n') || content.Contains('\r');

			if(mustQuote)
			{
				content = content.Replace("\"","\"\"");

				return $"\"{content}\"";
			}

			return content;
		}

		private static void _SaveCsvFile(string csvFolderPath,string csvFileName,string content)
		{
			var csvFilePath = Path.Combine(csvFolderPath,$"{csvFileName}.csv");

			FileUtility.WriteTextToFile(csvFilePath,content);
		}

		private bool _ShouldIncludeRow(string[] cellArray,int branchIndex,ExcelReader excelReader,string sheetName,int rowIndex,HashSet<string> keyHashSet,int keyIndex)
		{
			if(!m_branchSheet.IsIncludeRow(cellArray[branchIndex],excelReader.FilePath,sheetName,rowIndex))
			{
				return false;
			}

			var primaryKey = cellArray[keyIndex];

			if(keyHashSet.Contains(primaryKey))
			{
				throw new KZSheetException($"{primaryKey} is already added.",excelReader.FilePath,sheetName,rowIndex);
			}

			keyHashSet.Add(primaryKey);

			return true;
		}

		private static int _FindBranchIndex(string[] schemeArray,string filePath,string sheetName)
		{
			for(var i=0;i<schemeArray.Length;i++)
			{
				var scheme = schemeArray[i];

				if(string.Equals(scheme,c_branchTag))
				{
					return i;
				}
			}

			throw new KZSheetException($"$Branch is not included.",filePath,sheetName,0);
		}

		private string[][] _GenerateRowArray(ExcelReader excelReader,string sheetName,Dictionary<Type,List<int>> schemeIndexListDict)
		{
			var keyHashSet = new HashSet<string>();
			var schemeArray = excelReader.FindSchemeArray(sheetName);
			var branchIndex = _FindBranchIndex(schemeArray,excelReader.FilePath,sheetName);

			var rowSize = excelReader.GetRowSize(sheetName);
			var keyIndex = excelReader.FindPrimaryKeyIndex(sheetName);

			var rowList = new List<string[]>();
			var subSheetDict = schemeIndexListDict.Count == 0 ? [] : _GenerateSubSheetForCustomRow(excelReader,schemeIndexListDict);

			for(var i=c_valueIndex;i<rowSize;i++)
			{
				var cellArray = excelReader.FindCellArrayInRow(sheetName,i);

				// skip empty
				if(cellArray.Length == 0)
				{
					continue;
				}

				// check branch
				if(!_ShouldIncludeRow(cellArray,branchIndex,excelReader,sheetName,i,keyHashSet,keyIndex))
				{
					continue;
				}

				if(subSheetDict.Count != 0)
				{
					_ProcessCustomRow(cellArray,schemeIndexListDict,subSheetDict);
				}

				rowList.Add(cellArray);
			}

			return [..rowList];
		}

		private static Dictionary<string,string> _GenerateSubSheetForCustomRow(ExcelReader excelReader,Dictionary<Type,List<int>> schemeIndexListDict)
		{
			var dictionary = new Dictionary<string,string>();
			
			foreach(var pair in schemeIndexListDict)
			{
				var subSheetName = $"+{pair.Key.Name}";
				var schemeArray = excelReader.FindSchemeArray(subSheetName);

				var rowSize = excelReader.GetRowSize(subSheetName);
				var keyIndex = excelReader.FindPrimaryKeyIndex(subSheetName);

				for(var i=c_valueIndex;i<rowSize;i++)
				{
					var cellArray = excelReader.FindCellArrayInRow(subSheetName,i);

					// skip empty
					if(cellArray.Length == 0)
					{
						continue;
					}

					var primaryKey = cellArray[keyIndex];

					if(dictionary.ContainsKey(primaryKey))
					{
						throw new KZSheetException($"{primaryKey} is already added.",excelReader.FilePath,subSheetName,i);
					}

					var content = JsonConvert.SerializeObject(excelReader.Deserialize(schemeArray,pair.Key,cellArray,i),new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

					dictionary.Add($"{pair.Key.Name}_{primaryKey}",content);
				}
			}

			return dictionary;
		}

		private static void _ProcessCustomRow(string[] cellArray,Dictionary<Type,List<int>> schemeIndexListDict,Dictionary<string,string> subSheetDict)
		{
			foreach(var pair in schemeIndexListDict)
			{
				foreach(var index in pair.Value)
				{
					var subKey = $"{pair.Key.Name}_{cellArray[index]}";

					if(subSheetDict.TryGetValue(subKey,out var value))
					{
						cellArray[index] = value;
					}
				}
			}
		}

		private Type _GetProtoType(string className)
		{
			return (m_protoAssembly.GetType($"KZLib.KZData.{className}") ?? m_assembly.GetType($"KZLib.KZData.{className}")) ?? throw new InvalidDataException($"Invalid data in {className}");
		}
	}
}