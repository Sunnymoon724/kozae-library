using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using Newtonsoft.Json;
using UnityEngine;

namespace KZLib.ToolKits
{
	public record ExcelSchemeInfo
	{
		public string Title { get; }
		public int Index { get; }

		public ExcelSchemeInfo(string title,int index)
		{
			Title = title;
			Index = index;
		}
	}

	public class ExcelReader
	{
		public string FilePath { get; }

		private readonly Dictionary<string,string[][]> m_sheetDict = new();

		public ExcelReader(string filePath)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			if(!KZFileKit.IsFileExist(filePath))
			{
				throw new FileNotFoundException($"{filePath} is not exist");
			}

			FilePath = filePath;

			using var stream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
			using var workbook = new XLWorkbook(stream);

			var worksheets = workbook.Worksheets;

			for(var i=0;i<worksheets.Count;i++)
			{
				var worksheet = worksheets.Worksheet(i+1);

				var range = worksheet.RangeUsed();

				if(range == null)
				{
					continue;
				}

				var rowCount = range.RowCount();
				var rowArray = new string[rowCount][];

				for(var j=0;j<rowCount;j++)
				{
					var row = range.Row(j+1);
					var cellCount = row.CellCount();

					rowArray[j] = new string[cellCount];

					for(var k=0;k<cellCount;k++)
					{
						rowArray[j][k] = row.Cell(k+1).GetValue<string>() ?? string.Empty;
					}
				}

				m_sheetDict.Add(worksheet.Name,rowArray);
			}
		}

		public IReadOnlyCollection<string> SheetNameGroup => m_sheetDict.Keys;

		public string FindSheetName(Func<string,bool> condition)
		{
			foreach(var key in m_sheetDict.Keys)
			{
				if(condition.Invoke(key))
				{
					return key;
				}
			}

			throw new ArgumentNullException("SheetName is not exist in condition");
		}

		public string[] FindSheetNameArray(Func<string,bool> condition)
		{
			var nameList = new List<string>();

			foreach (var key in m_sheetDict.Keys)
			{
				if (condition.Invoke(key))
				{
					nameList.Add(key);
				}
			}

			return nameList.ToArray();
		}

		public bool IsExistSheetName(string sheetName)
		{
			return m_sheetDict.ContainsKey(sheetName);
		}

		private string[][] _GetRowArray(string sheetName)
		{
			if(!m_sheetDict.TryGetValue(sheetName,out var sheet))
			{
				throw new KeyNotFoundException($"Sheet {sheetName} does not exist.");
			}

			return sheet;
		}

		public int GetRowSize(string sheetName)
		{
			return _GetRowArray(sheetName).Length;
		}

		public int FindPrimaryKeyIndex(string sheetName)
		{
			var typeArray = FindCellArrayInRow(sheetName,0);

			for(var i=0;i<typeArray.Length;i++)
			{
				if(typeArray[i].Contains(":pk"))
				{
					return i;
				}
			}

			throw new KeyNotFoundException($"PrimaryKey does not exist in {sheetName}.");
		}

		/// <summary>
		/// Get data group in row
		/// </summary>
		public string[] FindCellArrayInRow(string sheetName,int index)
		{
			var rowArray = _GetRowArray(sheetName);

			_ValidateRange(sheetName,index,rowArray.Length);

			var cellArray = rowArray[index];

			return _IsExistRow(cellArray) ? cellArray : Array.Empty<string>();
		}

		/// <summary>
		/// Get data group in rows
		/// </summary>
		public string[][] MergeCellArrayInRows(string sheetName,params int[] indexArray)
		{
			var jaggedArray = new string[indexArray.Length][];

			for(var i=0;i<indexArray.Length;i++)
			{
				jaggedArray[i] = FindCellArrayInRow(sheetName,indexArray[i]);
			}

			return jaggedArray;
		}

		/// <summary>
		/// Get data group in column
		/// </summary>
		public string[] ExtractCellArrayInColumn(string sheetName,int index)
		{
			var rowArray = _GetRowArray(sheetName);

			_ValidateRange(sheetName,index,rowArray[0].Length);

			var length = rowArray.Length;
			var columnArray = new string[length];

			for(var i=0;i<length;i++)
			{
				columnArray[i] = (0 <= index && index < rowArray[i].Length) ? rowArray[i][index] : string.Empty;
			}

			return columnArray;
		}

		/// <summary>
		/// Get data group in columns
		/// </summary>
		public string[][] MergeCellArrayInColumns(string sheetName,params int[] indexArray)
		{
			var jaggedArray = new string[indexArray.Length][];

			for(var i=0;i<indexArray.Length;i++)
			{
				jaggedArray[i] = ExtractCellArrayInColumn(sheetName,indexArray[i]);
			}

			return jaggedArray;
		}

		public IEnumerable<TData> DeserializeGroup<TData>(string sheetName,int startRow = 1)
		{
			foreach(var result in DeserializeGroup(sheetName,typeof(TData),startRow))
			{
				yield return (TData) result;
			}
		}

		public IEnumerable<object> DeserializeGroup(string sheetName,Type dataType,int startRow = 1)
		{
			var rowArray = _GetRowArray(sheetName);
			var lastRow = rowArray.Length;
			startRow = Math.Clamp(startRow,0,lastRow);

			var schemeArray = FindSchemeArray(sheetName);

			// n -> last (get row)
			for(var i=startRow;i<lastRow;i++)
			{
				var cellArray = rowArray[i];

				if(!_IsExistRow(cellArray))
				{
					continue;
				}

				yield return Deserialize(schemeArray,dataType,cellArray,i);
			}
		}

		public TData Deserialize<TData>(string[] schemeArray,string[] cellArray,int line)
		{
			return (TData) Deserialize(schemeArray,typeof(TData),cellArray,line);
		}

		public object Deserialize(string[] schemeArray,Type dataType,string[] cellArray,int line)
		{
			var instance = Activator.CreateInstance(dataType);
			var schemeIndexList = new List<int>();

			foreach(var propertyInfo in dataType.GetProperties())
			{
				schemeIndexList.Clear();

				var propertyType = propertyInfo.PropertyType;
				var propertyName = propertyInfo.Name;

				for(var i=0;i<schemeArray.Length;i++)
				{
					var scheme = schemeArray[i].Replace(":pk","");

					if(string.Equals(scheme,propertyName))
					{
						schemeIndexList.Add(i);
					}
				}

				if(schemeIndexList.Count < 1)
				{
					throw new ArgumentNullException($"{propertyName} is not include in {string.Join("/",schemeArray)}");
				}

				if(propertyType.IsArray)
				{
					var elementType = propertyType.GetElementType();
					var resultArray = Array.CreateInstance(elementType,schemeIndexList.Count);

					for(var i=0;i<schemeIndexList.Count;i++)
					{
						var cell = cellArray[schemeIndexList[i]];

						resultArray.SetValue(_ConvertCell(cell,line,elementType),i);
					}

					propertyInfo.SetValue(instance,resultArray);
				}
				else
				{
					var cell = cellArray[schemeIndexList[0]];

					propertyInfo.SetValue(instance,_ConvertCell(cell,line,propertyType));
				}
			}

			return instance;
		}

		private object _ConvertCell(string cell,int line,Type targetType)
		{
			if(targetType == typeof(string))
			{
				return cell.Replace("\\n",Environment.NewLine);
			}

			if(targetType.IsEnum)
			{
				if(Enum.TryParse(targetType,cell,out var enumValue))
				{
					return enumValue;
				}
				else
				{
					throw new InvalidCastException(_CreateLog($"{cell} is not include in {targetType.Name}.",line,targetType));
				}
			}

			if(targetType == typeof(Vector2) || targetType == typeof(Vector3))
			{
				var vectorArray = cell.Trim('(', ')').Split(',');

				try
				{
					var valueArray = Array.ConvertAll(vectorArray,float.Parse);

					if(targetType == typeof(Vector2) && valueArray.Length == 2)
					{
						return new Vector2(valueArray[0],valueArray[1]);
					}

					if(targetType == typeof(Vector3) && valueArray.Length == 3)
					{
						return new Vector3(valueArray[0],valueArray[1],valueArray[2]);
					}

					throw new FormatException(_CreateLog($"{cell} is not a valid {targetType.Name}.",line,targetType));
				}
				catch(Exception exception)
				{
					throw new FormatException(_CreateLog($"{exception.Message} in {cell}.",line,targetType));
				}
			}

			if(targetType == typeof(DateTime))
			{
				if(string.Equals(cell,"DateTime.Now",StringComparison.OrdinalIgnoreCase))
				{
					return DateTime.Now;
				}

				var formatArray = new string[] {"yyyy-MM-dd HH:mm:ss","yyyy/MM/dd HH:mm:ss", };

				if(DateTime.TryParseExact(cell,formatArray,CultureInfo.InvariantCulture,DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,out DateTime parsedDate))
				{
					return parsedDate;
				}
				else
				{
					throw new InvalidCastException(_CreateLog($"{cell} is not a valid DateTime format.",line,targetType));
				}
			}

			if(targetType.IsPrimitive)
			{
				if(string.IsNullOrEmpty(cell))
				{
					return Activator.CreateInstance(targetType);
				}
				else
				{
					return Convert.ChangeType(cell,targetType);
				}
			}

			try
			{
                var result = JsonConvert.DeserializeObject(cell,targetType);

				if(result != null)
				{
					return result;
				}
			}
			catch(JsonException exception)
			{
				throw new InvalidCastException(_CreateLog($"{cell} convert is failed by json. ( exception : {exception.Message})",line,targetType));
			}

			throw new NotSupportedException(_CreateLog($"{cell} is not supported type.",line,targetType));
		}

		/// <summary>
		/// Get data group in range (x,y -> start point, w,h -> range size)
		/// </summary>
		public string[,] ConvertToArray(string sheetName,int x,int y,int width,int height)
		{
			var rowArray = _GetRowArray(sheetName);
			var resultArray = new string[width,height];

			for(var i=x;i<x+width;i++)
			{
				var cellArray = rowArray[i];

				for(var j=y;j<y+height;j++)
				{
					resultArray[i,j] = _IsValidCellArray(cellArray) ? cellArray[j] : string.Empty;
				}
			}

			return resultArray;
		}

		public IEnumerable<ExcelSchemeInfo> FindSchemeInfoGroup(string sheetName)
		{
			var schemeArray = FindSchemeArray(sheetName);

			for(var i=0;i<schemeArray.Length;i++)
			{
				var scheme = schemeArray[i];

				if(string.IsNullOrEmpty(scheme))
				{
					continue;
				}

				foreach(var keyword in s_keywordArray)
				{
					if(string.Equals(keyword,scheme))
					{
						throw new InvalidDataException($"{scheme} is invalid title.");
					}
				}

				yield return new ExcelSchemeInfo(scheme,i);
			}
		}

		/// <summary>
		/// Get title & index
		/// </summary>
		public string[] FindSchemeArray(string sheetName)
		{
			return FindCellArrayInRow(sheetName,0) ?? throw new NullReferenceException($"Scheme is not included in {sheetName}");
		}

		/// <summary>
		/// empty or %start -> not exist
		/// </summary>
		private bool _IsExistRow(string[] cellArray)
		{
			if(!_IsValidCellArray(cellArray))
			{
				return false;
			}

			var header = cellArray[0];

			return !header.StartsWith("%") && !string.IsNullOrEmpty(header);
		}

		private void _ValidateRange(string sheetName,int index,int count)
		{
			if(index < 0 || index >= count)
			{
				throw new IndexOutOfRangeException($"{index} is out of range in {sheetName}");
			}
		}

		private string _CreateLog(string log,int line,Type targetType)
		{
			return $"{log} [line: {line} / type: {targetType}]";
		}

		private bool _IsValidCellArray(string[] cellArray)
		{
			return cellArray != null && cellArray.Length != 0;
		}

		private static readonly string[] s_keywordArray = new string[]
		{
			"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", 
			"class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
			"event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
			"if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
			"namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected",
			"public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", 
			"string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
			"ushort", "using", "virtual", "void", "volatile", "while",
		};
	}
}