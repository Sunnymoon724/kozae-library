using System.Reflection;
using System.Text;
using KZConsole.KZProto;
using KZLib.KZTool;
using MessagePack;
using Newtonsoft.Json;

namespace KZConsole
{
	public class ProtoGenerator
	{
		private const int c_type_index = 1;
		private const int c_branch_index = 1;
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

			Console.WriteLine("-Make branch.");

			m_branchSheet.GenerateBranch();

			Console.WriteLine("-Compile code");

			m_protoAssembly = CodeCompiler.CompileToAssembly(codeGroup);

			Console.WriteLine("Generate proto.");
			Console.WriteLine("-Convert csv file.");

			_GenerateProto(protoFilePathList,outputFolderPath);
		}

		private void _GenerateProto(List<string> protoFilePathList,string outputFolderPath)
		{
			var csvFolderPath = Path.Combine(outputFolderPath,"Csv");
			var byteFolderPath = Path.Combine(outputFolderPath,"Proto");

			for(var i=0;i<protoFilePathList.Count;i++)
			{
				var protoFilePath = protoFilePathList[i];
				var fileName = Path.GetFileNameWithoutExtension(protoFilePath);

				if(s_exception_file_name_array.Contains(fileName))
				{
					continue;
				}

				var excelReader = new ExcelReader(protoFilePath);
				var protoList = GenerateProtoList(fileName,excelReader,csvFolderPath);

				if(protoList.Count == 0)
				{
					continue;
				}

				var serialize = MessagePackSerializer.Serialize(protoList);
				var filePath = Path.Combine(byteFolderPath,$"{fileName}.bytes");

				Utility.WriteBytesToFile(filePath,serialize);

				Console.WriteLine($"-Save {fileName} proto");
			}
		}

		private List<object> GenerateProtoList(string fileName,ExcelReader excelReader,string csvFolderPath)
		{
			var protoName = $"{fileName}Proto";

			Console.WriteLine($"-Generate {fileName}");

			var sheetNameArray = excelReader.FindSheetNameArray(x=>x.StartsWith('+'));
			var sheetNameCount = sheetNameArray.Length;

			if(sheetNameCount < 1)
			{
				Console.WriteLine($"Warning : {fileName} is not include +Sheet");

				return [];
			}

			var protoType = GetDataType(protoName);
			var mainSheetName = sheetNameArray[0];
			var schemeIndexListDict = new Dictionary<Type,List<int>>();

			//? Check sub-sheet
			if(sheetNameCount != 1)
			{
				var typeNameArray = excelReader.GetCellArrayInRow(mainSheetName,Global.EXCEL_TYPE_INDEX);

				for(var i=1;i<sheetNameCount;i++)
				{
					var sheetName = Utility.RemovePlusHeader(sheetNameArray[i]);
					var className = $"{fileName}{sheetName}";

					for(var j=0;j<typeNameArray.Length;j++)
					{
						var typeName = typeNameArray[j].Replace("[]","");

						if(string.Equals(typeName,className))
						{
							var subClassType = GetDataType(className);

							if(!schemeIndexListDict.TryGetValue(subClassType,out List<int>? indexList))
							{
								indexList = [];
								schemeIndexListDict.Add(subClassType,indexList);
							}

							indexList.Add(j);
						}
					}
				}
			}

			var rowArray = (sheetNameCount == 1 || schemeIndexListDict.Count == 0) ? GenerateRowArray(excelReader,mainSheetName) : GenerateCustomRowArray(excelReader,mainSheetName,schemeIndexListDict);

			var dataList = GenerateExcelToDataList(excelReader,rowArray,mainSheetName,protoType,out var csvText);
			var csvFilePath = Path.Combine(csvFolderPath,$"{fileName}.csv");

			Utility.WriteTextToFile(csvFilePath,csvText);

			return dataList;
		}

		private static List<object> GenerateExcelToDataList(ExcelReader excelReader,string[][] rowArray,string sheetName,Type dataType,out string csvText)
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

		private string[][] GenerateRowArray(ExcelReader excelReader,string sheetName)
		{
			var keyHashSet = new HashSet<string>();
			var schemeArray = excelReader.GetSchemeArray(sheetName);

			var branchIndex = FindCellIndex(schemeArray,"#Branch");

			var rowSize = excelReader.GetRowSize(sheetName);
			var keyIndex = excelReader.FindPrimaryKeyIndex(sheetName);

			var rowList = new List<string[]>();

			for(var i=c_value_index;i<rowSize;i++)
			{
				var cellArray = excelReader.GetCellArrayInRow(sheetName,i);

				// skip # or empty
				if(cellArray.Length == 0)
				{
					continue;
				}

				// check branch
				if(branchIndex != -1 && !m_branchSheet.IsIncludeRow(cellArray[branchIndex],excelReader.FilePath,sheetName,i))
				{
					continue;
				}

				var primaryKey = cellArray[keyIndex];

				if(keyHashSet.Contains(primaryKey))
				{
					throw new SheetConvertException($"{primaryKey} is already added.",excelReader.FilePath,sheetName,i);
				}

				keyHashSet.Add(primaryKey);

				rowList.Add(cellArray);
			}

			return [..rowList];
		}

		private static int FindCellIndex(string[] schemeArray,string cell)
		{
			for(var i=0;i<schemeArray.Length;i++)
			{
				var scheme = schemeArray[i];

				if(string.Equals(scheme,cell))
				{
					return i;
				}
			}

			return -1;
		}

		private string[][] GenerateCustomRowArray(ExcelReader excelReader,string mainSheetName,Dictionary<Type,List<int>> schemeIndexListDict)
		{
			var keyHashSet = new HashSet<string>();
			var protoSchemeArray = excelReader.GetSchemeArray(mainSheetName);

			var branchIndex = FindCellIndex(protoSchemeArray,"#Branch");

			var rowSize = excelReader.GetRowSize(mainSheetName);
			var keyIndex = excelReader.FindPrimaryKeyIndex(mainSheetName);

			var rowList = new List<string[]>();

			var subSheetDataDict = new Dictionary<string,string>();

			foreach(var pair in schemeIndexListDict)
			{
				var subName = $"+{pair.Key.Name}";
				var subSheetName = $"+{subName.Replace(mainSheetName,"")}";

				AddSubSheetData(ref subSheetDataDict,excelReader,subSheetName,pair.Key);
			}

			for(var i=c_value_index;i<rowSize;i++)
			{
				var cellArray = excelReader.GetCellArrayInRow(mainSheetName,i);

				// skip # or empty
				if(cellArray.Length == 0)
				{
					continue;
				}

				// check branch
				if(branchIndex != -1 && !m_branchSheet.IsIncludeRow(cellArray[branchIndex],excelReader.FilePath,mainSheetName,i))
				{
					continue;
				}

				var primaryKey = cellArray[keyIndex];

				if(keyHashSet.Contains(primaryKey))
				{
					throw new SheetConvertException($"{primaryKey} is already added.",excelReader.FilePath,mainSheetName,i);
				}

				keyHashSet.Add(primaryKey);

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

				rowList.Add(cellArray);
			}

			return [..rowList];
		}

		private static void AddSubSheetData(ref Dictionary<string,string> dataDict,ExcelReader excelReader,string sheetName,Type dataType)
		{
			var schemeArray = excelReader.GetSchemeArray(sheetName);

			var rowSize = excelReader.GetRowSize(sheetName);
			var keyIndex = excelReader.FindPrimaryKeyIndex(sheetName);

			for(var i=c_value_index;i<rowSize;i++)
			{
				var cellArray = excelReader.GetCellArrayInRow(sheetName,i);

				// skip # or empty
				if(cellArray.Length == 0)
				{
					continue;
				}

				var primaryKey = cellArray[keyIndex];

				if(dataDict.ContainsKey(primaryKey))
				{
					throw new SheetConvertException($"{primaryKey} is already added.",excelReader.FilePath,sheetName,i);
				}

				var text = JsonConvert.SerializeObject(excelReader.Deserialize(schemeArray,dataType,cellArray,i),new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

				dataDict.Add($"{dataType.Name}_{primaryKey}",text);
			}
		}

		private Type GetDataType(string className)
		{
			var type = m_protoAssembly.GetType($"KZLib.KZData.{className}");

			if(type != null)
			{
				return type;
			}

			type = m_dataAssembly.GetType($"KZLib.KZData.{className}");

			if(type != null)
			{
				return type;
			}

			throw new InvalidDataException($"Invalid data in {className}");
		}
	}
}