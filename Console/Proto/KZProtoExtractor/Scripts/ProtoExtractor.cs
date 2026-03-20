using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using KZConsole.Utilities;
using KZLib.ToolKits;
using KZLib.Utilities;
using MemoryPack;
using Newtonsoft.Json;

namespace KZConsole
{
	public class ProtoExtractor(string branchName, string branchFilePath)
	{
		private const int c_valueIndex = 3;

		private readonly BranchGenerator m_branchGenerator = new(branchName,branchFilePath);
		private readonly Assembly m_assembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"KZProto.dll"));

		public void ExtractAllProto(List<string> protoFilePathList)
		{
			KZCommonKit.WriteLog("Extract all proto.",LogType.Info);

			_GenerateBranch();
			_ExtractProtoAllFiles(protoFilePathList);
		}

		private void _GenerateBranch()
		{
			KZCommonKit.WriteLog("-Make branch.",LogType.Info);

			m_branchGenerator.GenerateBranch();
		}

		private void _ExtractProtoAllFiles(List<string> protoFilePathList)
		{
			KZCommonKit.WriteLog("Extract proto.",LogType.Info);

			var outputFolderPath = Path.Combine(KZFileKit.GetProjectParentPath(),"ProtoOutput");

			var csvFolderPath = Path.Combine(outputFolderPath,"Csv");
			var byteFolderPath = Path.Combine(outputFolderPath,"Proto");

			var excludeFileNameList = new List<string>
			{
				"Enum",
				"Branch",
			};

			foreach(var protoFilePath in protoFilePathList)
			{
				var fileName = KZFileKit.GetOnlyName(protoFilePath);

				if(excludeFileNameList.Contains(fileName))
				{
					continue;
				}

				_ExtractProtoFile(protoFilePath,csvFolderPath,byteFolderPath);
			}
		}

		private void _ExtractProtoFile(string protoFilePath,string csvFolderPath,string byteFolderPath)
		{
			var fileName = KZFileKit.GetOnlyName(protoFilePath);
			var excelReader = new ExcelReader(protoFilePath);

			var protoType = _GetProtoType($"{fileName}Proto");

			if(_TryExtractProtoArray(fileName,protoType,excelReader,out var protoArray,out var backupText))
			{
				_SaveCsvFile(csvFolderPath,fileName,backupText);
				_SaveProto(fileName,protoArray,byteFolderPath);

				KZCommonKit.WriteLog($"-Save {fileName} proto",LogType.Info);
			}
		}

		private static void _SaveProto(string fileName,Array protoArray,string byteFolderPath)
		{
			var arrayType = protoArray.GetType();

			var serialized = MemoryPackSerializer.Serialize(arrayType,protoArray);

			var filePath = Path.Combine(byteFolderPath,$"{fileName}.bytes");

			KZFileKit.WriteByteToFile(filePath,serialized);
		}

		private static void _SaveCsvFile(string csvFolderPath,string csvFileName,string backupText)
		{
			var csvFilePath = Path.Combine(csvFolderPath,$"{csvFileName}.csv");

			KZFileKit.WriteTextToFile(csvFilePath,backupText);
		}

		private bool _TryExtractProtoArray(string fileName,Type protoType,ExcelReader excelReader,out Array protoArray,out string backupText)
		{
			KZCommonKit.WriteLog($"-Extract {fileName}",LogType.Info);

			static bool _FindPlus(string sheetName)
			{
				return sheetName.StartsWith('+');
			}

			// extract is only +sheet
			var sheetNameArray = excelReader.FindSheetNameArray(_FindPlus);

			if(sheetNameArray.Length < 1)
			{
				KZCommonKit.WriteLog($"Warning : {fileName} is not include +Sheet",LogType.Info);

				backupText = string.Empty;
				protoArray = Array.Empty<object>();

				return false;
			}

			// first sheet is main class (others are field class)
			var mainSheetName = sheetNameArray[0];

			// extract field class (support main class)
			var schemeIndexListDict = _ExtractSubSheet(excelReader,sheetNameArray);

			var rowArray = _ExtractRowArray(excelReader,mainSheetName,schemeIndexListDict);
			protoArray = _ConvertProtoArray(excelReader,rowArray,mainSheetName,protoType,out backupText);

			return true;
		}

		private Dictionary<Type,List<int>> _ExtractSubSheet(ExcelReader excelReader,string[] sheetNameArray)
		{
			if(sheetNameArray.Length == 1)
			{
				return [];
			}

			var indexListDict = new Dictionary<Type,List<int>>();
			var typeNameArray = excelReader.FindCellArrayInRow(sheetNameArray[0],1);

			for(var i=1;i<sheetNameArray.Length;i++)
			{
				var className = sheetNameArray[i].TrimStart('+');

				_ExtractSubSheetTypes(typeNameArray,className,ref indexListDict);
			}

			return indexListDict;
		}

		private void _ExtractSubSheetTypes(string[] typeNameArray,string className,ref Dictionary<Type,List<int>> schemeIndexListDict)
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
				string[] currentRow = rowArray[i];
				string[] escapedRow = new string[currentRow.Length];

				for(var j=0;j<currentRow.Length;j++)
				{
					escapedRow[j] = _EscapeText(currentRow[j]);
				}

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

		private bool _ShouldIncludeRow(string[] cellArray,int branchIndex,ExcelReader excelReader,string sheetName,int rowIndex,HashSet<string> keyHashSet,int keyIndex)
		{
			if(!m_branchGenerator.IsIncludeRow(cellArray[branchIndex],excelReader.FilePath,sheetName,rowIndex))
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

				if(string.Equals(scheme,"%Branch"))
				{
					return i;
				}
			}

			throw new KZSheetException("%Branch is not included.",filePath,sheetName,0);
		}

		private string[][] _ExtractRowArray(ExcelReader excelReader,string sheetName,Dictionary<Type,List<int>> schemeIndexListDict)
		{
			var keyHashSet = new HashSet<string>();
			var schemeArray = excelReader.FindSchemeArray(sheetName);
			var branchIndex = _FindBranchIndex(schemeArray,excelReader.FilePath,sheetName);

			var rowSize = excelReader.GetRowSize(sheetName);
			var keyIndex = excelReader.FindPrimaryKeyIndex(sheetName);

			var rowList = new List<string[]>();
			var subSheetDict = schemeIndexListDict.Count == 0 ? [] : _ExtractSubSheetForCustomRow(excelReader,schemeIndexListDict);

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
					_ExtractCustomRow(cellArray,schemeIndexListDict,subSheetDict);
				}

				rowList.Add(cellArray);
			}

			return [..rowList];
		}

		private static Dictionary<string,string> _ExtractSubSheetForCustomRow(ExcelReader excelReader,Dictionary<Type,List<int>> schemeIndexListDict)
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

		private static void _ExtractCustomRow(string[] cellArray,Dictionary<Type,List<int>> schemeIndexListDict,Dictionary<string,string> subSheetDict)
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
			return m_assembly.GetType($"KZLib.Data.{className}") ?? throw new InvalidDataException($"Invalid data in {className}");
		}
	}
}