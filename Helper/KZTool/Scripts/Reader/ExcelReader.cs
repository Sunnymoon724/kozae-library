using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDataReader;
using KZLib.KZUtility;
using Newtonsoft.Json;
using UnityEngine;

namespace KZLib.KZTool
{
	public class ExcelReader
	{
		private const int c_scheme_index = 0;

		public string FilePath { get; }

		private readonly Dictionary<string,string[][]> m_sheetDict = new Dictionary<string,string[][]>();

		public ExcelReader(string filePath)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			if(!FileUtility.IsFileExist(filePath))
			{
				throw new FileNotFoundException($"{filePath} is not exist");
			}

			using var stream = new FileStream(filePath,FileMode.Open,FileAccess.Read);
			using var reader = ExcelReaderFactory.CreateReader(stream);

			FilePath = filePath;

			var dataSet = reader.AsDataSet();
			var sheetCollection = dataSet.Tables;

			for(var i=0;i<sheetCollection.Count;i++)
			{
				var sheet = sheetCollection[i];
				var rowCollection = sheet.Rows;

				var rowCount = rowCollection.Count;
				var rowArray = new string[rowCount][];

				for(var j=0;j<rowCount;j++)
				{
					var cellArray = rowCollection[j].ItemArray;
					var cellCount = cellArray.Length;

					rowArray[j] = new string[cellCount];

					for(var k=0;k<cellCount;k++)
					{
						rowArray[j][k] = cellArray[k]?.ToString() ?? string.Empty;
					}
				}

				m_sheetDict.Add(sheet.TableName,rowArray);
			}
		}

		public IEnumerable<string> SheetNameGroup => m_sheetDict.Keys;

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

		private string[][] GetRowArray(string sheetName)
		{
			if(!m_sheetDict.TryGetValue(sheetName,out var sheet))
			{
				throw new KeyNotFoundException($"Sheet '{sheetName}' does not exist.");
			}

			return sheet;
		}

		public int GetRowSize(string sheetName)
		{
			return GetRowArray(sheetName).Length;
		}

		public int FindPrimaryKeyIndex(string sheetName)
		{
			var schemeArray = GetSchemeArray(sheetName);

			for(var i=0;i<schemeArray.Length;i++)
			{
				if(schemeArray[i].Contains(":pk"))
				{
					return i;
				}
			}

			throw new KeyNotFoundException($"PrimaryKey does not exist in {sheetName}.");
		}

		/// <summary>
		/// Get data group in row
		/// </summary>
		public string[] GetCellArrayInRow(string sheetName,int index)
		{
			var rowArray = GetRowArray(sheetName);

			ValidateRange(sheetName,index,rowArray.Length);

			var cellArray = rowArray[index];

			return IsExistRow(cellArray) ? cellArray : Array.Empty<string>();
		}

		/// <summary>
		/// Get data group in rows
		/// </summary>
		public string[][] MergeCellArrayInRows(string sheetName,params int[] indexArray)
		{
			var jaggedArray = new string[indexArray.Length][];

			for(var i=0;i<indexArray.Length;i++)
			{
				jaggedArray[i] = GetCellArrayInRow(sheetName,indexArray[i]);
			}

			return jaggedArray;
		}

		/// <summary>
		/// Get data group in column
		/// </summary>
		public string[] ExtractCellArrayInColumn(string sheetName,int index)
		{
			var rowArray = GetRowArray(sheetName);

			ValidateRange(sheetName,index,rowArray[0].Length);

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
			var rowArray = GetRowArray(sheetName);
			var lastRow = rowArray.Length;
			startRow = Math.Clamp(startRow,0,lastRow);

			var schemeArray = GetSchemeArray(sheetName);

			// n -> last (get row)
			for(var i=startRow;i<lastRow;i++)
			{
				var cellArray = rowArray[i];

				if(!IsExistRow(cellArray))
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
					var scheme = schemeArray[i].Split(':')[0];

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

						resultArray.SetValue(ConvertCell(cell,line,elementType),i);
					}

					propertyInfo.SetValue(instance,resultArray);
				}
				else
				{
					var cell = cellArray[schemeIndexList[0]];

					propertyInfo.SetValue(instance,ConvertCell(cell,line,propertyType));
				}
			}

			return instance;
		}

		private object ConvertCell(string cell,int line,Type targetType)
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
					throw new InvalidCastException($"{cell} is not include in {targetType.Name}. [line : {line} / type : {targetType}]");
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

					throw new FormatException($"{cell} is not a valid {targetType.Name}. [line: {line} / type: {targetType}]");
				}
				catch(Exception exception)
				{
					throw new FormatException($"{exception.Message} in {cell}. [line: {line} / type: {targetType}]");
				}
			}

			if(targetType == typeof(DateTime))
			{
				return string.Equals(cell,"DateTime.Now") ? DateTime.Now : DateTime.Parse(cell);
			}

			if(targetType.IsPrimitive)
			{
				return Convert.ChangeType(cell,targetType);
			}

			try
			{
				var json = JsonConvert.DeserializeObject(cell,targetType);

				if(json != null)
				{
					return json;
				}
			}
			catch(JsonException exception)
			{
				throw new InvalidCastException($"{cell} convert is failed by json. [line : {line} / type : {targetType}] ({exception.Message})");
			}

			throw new NotSupportedException($"{cell} is not supported type. [line: {line} / type: {targetType}]");
		}

		/// <summary>
		/// Get data group in range (x,y -> start point, w,h -> range size)
		/// </summary>
		public string[,] ConvertToArray(string sheetName,int x,int y,int width,int height)
		{
			var rowArray = GetRowArray(sheetName);
			var resultArray = new string[width,height];

			for(var i=x;i<x+width;i++)
			{
				var cellArray = rowArray[i];

				for(var j=y;j<y+height;j++)
				{
					resultArray[i,j] = (cellArray == null || cellArray.Length == 0) ? string.Empty : cellArray[j];
				}
			}

			return resultArray;
		}

		public IEnumerable<(string Title,int Index)> GetSchemeGroup(string sheetName)
		{
			var schemeArray = GetSchemeArray(sheetName);

			for(var i=0;i<schemeArray.Length;i++)
			{
				var scheme = schemeArray[i];

				if(string.IsNullOrEmpty(scheme))
				{
					continue;
				}

				foreach(var keyword in s_keyword_array)
				{
					if(string.Equals(keyword,scheme))
					{
						throw new InvalidDataException($"{scheme} is invalid title.");
					}
				}

				yield return (scheme,i);
			}
		}

		/// <summary>
		/// Get title & index
		/// </summary>
		public string[] GetSchemeArray(string sheetName)
		{
			return GetCellArrayInRow(sheetName,c_scheme_index) ?? throw new NullReferenceException($"Scheme is not included in {sheetName}");
		}

		/// <summary>
		/// empty or %start -> not exist
		/// </summary>
		private bool IsExistRow(string[] cellArray)
		{
			if(cellArray == null || cellArray.Length == 0)
			{
				return false;
			}

			var header = cellArray[0];

			return !header.StartsWith("%") && !string.IsNullOrEmpty(header);
		}

		private void ValidateRange(string sheetName,int index,int count)
		{
			if(index < 0 || index >= count)
			{
				throw new IndexOutOfRangeException($"{index} is out of range in {sheetName}");
			}
		}

		private static readonly string[] s_keyword_array = new string[]
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