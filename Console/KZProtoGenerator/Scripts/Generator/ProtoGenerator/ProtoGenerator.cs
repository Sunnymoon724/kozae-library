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
		private const int c_value_index = 3;

		private static readonly string[] s_exception_file_name_array = ["Branch","Enum"];

		private readonly BranchSheet m_branchSheet = null!;

		private Assembly m_protoAssembly = null!;
		private readonly Assembly m_dataAssembly = null!;

		public ProtoGenerator(string branchName,string branchFilePath)
		{
			m_branchSheet = new(branchName,branchFilePath);

			var dataDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"KZData.dll");

			m_dataAssembly = Assembly.LoadFrom(dataDllPath);
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

			return s_exception_file_name_array.Contains(fileName);
		}

		private void _ProcessProtoFile(string protoFilePath,string csvFolderPath,string byteFolderPath)
		{
			var fileName = Path.GetFileNameWithoutExtension(protoFilePath);
			var excelReader = new ExcelReader(protoFilePath);
			var protoList = _GenerateProtoList(fileName,excelReader,csvFolderPath);

			if(protoList.Count > 0)
			{
                _SaveProtoData(fileName,protoList,byteFolderPath);

				Console.WriteLine($"-Save {fileName} proto");
			}
		}

		private static void _SaveProtoData(string fileName,List<object> protoList,string byteFolderPath)
		{
			var serializedData = MessagePackSerializer.Serialize(protoList);
			var filePath = Path.Combine(byteFolderPath,$"{fileName}.bytes");

			FileUtility.WriteByteToFile(filePath,serializedData);
		}

		private List<object> _GenerateProtoList(string fileName,ExcelReader excelReader,string csvFolderPath)
		{
			Console.WriteLine($"-Generate {fileName}");

			var sheetNameArray = excelReader.FindSheetNameArray(x => x.StartsWith('+'));

			if(sheetNameArray.Length < 1)
			{
				Console.WriteLine($"Warning : {fileName} is not include +Sheet");

				return [];
			}

			var protoType = _GetDataType($"{fileName}Proto");
			var mainSheetName = sheetNameArray[0];
			var schemeIndexListDict = _GenerateSubSheet(excelReader,sheetNameArray,fileName);

			var rowArray = (sheetNameArray.Length == 1 || schemeIndexListDict.Count == 0) ? _GenerateRowArray(excelReader,mainSheetName) : _GenerateCustomRowArray(excelReader,mainSheetName,schemeIndexListDict);

            var dataList = _GenerateDataList(excelReader,rowArray,mainSheetName,protoType,out var csvText);

            _SaveCsvFile(csvFolderPath,fileName,csvText);

            return dataList;
		}

		private Dictionary<Type,List<int>> _GenerateSubSheet(ExcelReader excelReader,string[] sheetNameArray,string fileName)
		{
			if(sheetNameArray.Length == 1)
			{
				return [];
			}

			var indexListDict = new Dictionary<Type,List<int>>();
			var typeNameArray = excelReader.GetCellArrayInRow(sheetNameArray[0],Global.EXCEL_TYPE_INDEX);

			for(var i=1;i<sheetNameArray.Length;i++)
			{
				var className = $"{sheetNameArray[i].TrimStart('+')}";

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
					var subClassType = _GetDataType(className);

					if(!schemeIndexListDict.TryGetValue(subClassType,out List<int>? value))
					{
						value = [];
						schemeIndexListDict[subClassType] = value;
					}

					value.Add(j);
				}
			}
		}

		private static List<object> _GenerateDataList(ExcelReader excelReader,string[][] rowArray,string sheetName,Type dataType,out string csvText)
		{
			var dataList = new List<object>();
			var schemeArray = excelReader.GetSchemeArray(sheetName);

			var csvBuilder = new StringBuilder();
			csvBuilder.AppendLine(string.Join(",",schemeArray));

			for(var i=0;i<rowArray.Length;i++)
			{
				csvBuilder.AppendLine(string.Join(",",rowArray[i]));

				dataList.Add(excelReader.Deserialize(schemeArray,dataType,rowArray[i],i));
			}

			csvText = csvBuilder.ToString();

			return dataList;
		}

		private static void _SaveCsvFile(string csvFolderPath,string fileName,string csvText)
		{
			var csvFilePath = Path.Combine(csvFolderPath,$"{fileName}.csv");

			FileUtility.WriteTextToFile(csvFilePath,csvText);
		}

		private string[][] _GenerateRowArray(ExcelReader excelReader,string sheetName)
		{
			var keyHashSet = new HashSet<string>();
			var schemeArray = excelReader.GetSchemeArray(sheetName);
			var branchIndex = _FindBranchIndex(schemeArray,excelReader.FilePath,sheetName);

			var rowSize = excelReader.GetRowSize(sheetName);
			var keyIndex = excelReader.FindPrimaryKeyIndex(sheetName);

			var rowList = new List<string[]>();

			for(var i=c_value_index;i<rowSize;i++)
			{
				var cellArray = excelReader.GetCellArrayInRow(sheetName,i);

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

				rowList.Add(cellArray);
			}

			return [..rowList];
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
				throw new SheetConvertException($"{primaryKey} is already added.",excelReader.FilePath,sheetName,rowIndex);
			}

			keyHashSet.Add(primaryKey);

			return true;
		}

		private static int _FindBranchIndex(string[] schemeArray,string filePath,string sheetName)
		{
			for(var i=0;i<schemeArray.Length;i++)
			{
				var scheme = schemeArray[i];

				if(string.Equals(scheme,"%Branch"))
				{
					return i;
				}
			}

			throw new SheetConvertException($"$Branch is not included.",filePath,sheetName,0);
		}

		private string[][] _GenerateCustomRowArray(ExcelReader excelReader,string mainSheetName,Dictionary<Type,List<int>> schemeIndexListDict)
        {
            var keyHashSet = new HashSet<string>();
            var schemeArray = excelReader.GetSchemeArray(mainSheetName);
            var branchIndex = _FindBranchIndex(schemeArray,excelReader.FilePath,mainSheetName);

            var rowSize = excelReader.GetRowSize(mainSheetName);
			var keyIndex = excelReader.FindPrimaryKeyIndex(mainSheetName);

			var rowList = new List<string[]>();
			var subSheetDataDict = _GenerateSubSheetDataForCustomRow(excelReader,schemeIndexListDict);

			for(var i=c_value_index;i<rowSize;i++)
			{
				var cellArray = excelReader.GetCellArrayInRow(mainSheetName,i);

				// skip empty
				if(cellArray.Length == 0)
				{
					continue;
				}

				// check branch
				if(!_ShouldIncludeRow(cellArray,branchIndex,excelReader,mainSheetName,i,keyHashSet,keyIndex))
				{
					continue;
				}

				_ProcessCustomRowData(cellArray,schemeIndexListDict,subSheetDataDict);

				rowList.Add(cellArray);
			}

			return [..rowList];
		}

		private static Dictionary<string,string> _GenerateSubSheetDataForCustomRow(ExcelReader excelReader,Dictionary<Type,List<int>> schemeIndexListDict)
		{
			var dataDict = new Dictionary<string,string>();
			
			foreach(var pair in schemeIndexListDict)
			{
				var subSheetName = $"+{pair.Key.Name}";
				var schemeArray = excelReader.GetSchemeArray(subSheetName);

				var rowSize = excelReader.GetRowSize(subSheetName);
				var keyIndex = excelReader.FindPrimaryKeyIndex(subSheetName);

				for(var i=c_value_index;i<rowSize;i++)
				{
					var cellArray = excelReader.GetCellArrayInRow(subSheetName,i);

					// skip empty
					if(cellArray.Length == 0)
					{
						continue;
					}

					var primaryKey = cellArray[keyIndex];

					if(dataDict.ContainsKey(primaryKey))
					{
						throw new SheetConvertException($"{primaryKey} is already added.",excelReader.FilePath,subSheetName,i);
					}

					var text = JsonConvert.SerializeObject(excelReader.Deserialize(schemeArray,pair.Key,cellArray,i),new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

					dataDict.Add($"{pair.Key.Name}_{primaryKey}",text);
				}
			}

			return dataDict;
		}

		private static void _ProcessCustomRowData(string[] cellArray,Dictionary<Type,List<int>> schemeIndexListDict,Dictionary<string,string> subSheetDataDict)
		{
			foreach(var pair in schemeIndexListDict)
			{
				foreach(var index in pair.Value)
				{
					var subDataKey = $"{pair.Key.Name}_{cellArray[index]}";

					if(subSheetDataDict.TryGetValue(subDataKey,out var value))
					{
						cellArray[index] = value;
					}
				}
			}
		}

		private Type _GetDataType(string className)
		{
			return (m_protoAssembly.GetType($"KZLib.KZData.{className}") ?? m_dataAssembly.GetType($"KZLib.KZData.{className}")) ?? throw new InvalidDataException($"Invalid data in {className}");
		}
	}
}