// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.IO;
// using System.Reflection;
// using ExcelDataReader;
// using UnityEngine;

// namespace KZLib.Tool
// {
// 	public class ExcelReader
// 	{
// 		private readonly DataSet? m_DataSet = null;

// 		public ExcelReader(string _filePath)
// 		{
// 			if(!File.Exists(_filePath))
// 			{
// 				throw new NullReferenceException($"{_filePath} is not file path");
// 			}

// 			using var stream = new FileStream(_filePath,FileMode.Open,FileAccess.Read);
// 			using var reader = ExcelReaderFactory.CreateReader(stream);

// 			m_DataSet = reader.AsDataSet();
// 		}

// 		public static DataTable? ParseCsv(Stream inputStream)
//         {
//             using var reader = ExcelReaderFactory.CreateCsvReader(inputStream);
//             var tables = reader.AsDataSet().Tables;
//             return tables.Count > 0 ? tables[0] : null;
//         }

// 		private DataTableCollection TableCollection
// 		{
// 			get
// 			{
// 				if(m_DataSet == null)
// 				{
// 					throw new NullReferenceException("DataSet is null in ExcelReader");
// 				}

// 				return m_DataSet.Tables;
// 			}
// 		}

// 		public IEnumerable<string> SheetNameGroup
// 		{
// 			get
// 			{
// 				var tableCollection = TableCollection;

// 				for(var i=0;i<tableCollection.Count;i++)
// 				{
// 					yield return tableCollection[i].TableName;
// 				}
// 			}
// 		}

// 		private DataTable GetSheet(string _sheetName)
// 		{
// 			var tableCollection = TableCollection;

// 			if(!tableCollection.Contains(_sheetName))
// 			{
// 				throw new NullReferenceException($"The sheet '{_sheetName}' does not exist in tableCollection.");
// 			}

// 			return tableCollection[_sheetName];
// 		}

// 		/// <summary>
// 		/// empty or #start -> not exist
// 		/// </summary>
// 		public bool IsExistRowArray(string _sheetName,int _idx)
// 		{
// 			var rowCollection = GetSheet(_sheetName).Rows;

// 			if(_idx < 0 || _idx >= rowCollection.Count)
// 			{
// 				throw new IndexOutOfRangeException($"{_idx} is out of range in rowCollection. [{_sheetName}]");
// 			}

// 			var cellArray = rowCollection[_idx].ItemArray;

// 			if(cellArray == null || cellArray.Length == 0)
// 			{
// 				throw new InvalidDataException($"ItemArray is null for row {_idx} in sheet [{_sheetName}]");
// 			}

// 			var header = cellArray[0].ToString();

// 			return !header.Contains("#") && !string.IsNullOrEmpty(header);
// 		}

// 		/// <summary>
// 		/// Get data group in row
// 		/// </summary>
// 		public string[]? GetRowArray(string _sheetName,int _idx)
// 		{
// 			var rowCollection = GetSheet(_sheetName).Rows;

// 			if(_idx < 0 || _idx >= rowCollection.Count)
// 			{
// 				throw new IndexOutOfRangeException($"{_idx} is out of range in rowCollection. [{_sheetName}]");
// 			}

// 			var cellArray = rowCollection[_idx].ItemArray;

// 			if(cellArray == null || cellArray.Length == 0)
// 			{
// 				throw new InvalidDataException($"ItemArray is null for row {_idx} in sheet [{_sheetName}]");
// 			}

// 			if(!IsExistRowArray(_sheetName,0))
// 			{
// 				return null;
// 			}

// 			var rowArray = new string[cellArray.Length];

// 			for(var i=0;i<cellArray.Length;i++)
// 			{
// 				rowArray[i] = cellArray[i].ToString();
// 			}

// 			return rowArray;
// 		}

// 		/// <summary>
// 		/// Get data group in rows
// 		/// </summary>
// 		public string[][] GetRowJaggedArray(string _sheetName,params int[] _idxArray)
// 		{
// 			var jaggedList = new List<string[]>(_idxArray.Length);

// 			for(var i=0;i<_idxArray.Length;i++)
// 			{
// 				var rowArray = GetRowArray(_sheetName,_idxArray[i]);

// 				if(rowArray == null || rowArray.Length == 0)
// 				{
// 					continue;
// 				}

// 				jaggedList.Add(rowArray);
// 			}

// 			return jaggedList.ToArray();
// 		}

// 		/// <summary>
// 		/// Get data group in column
// 		/// </summary>
// 		public string[] GetColumnArray(string _sheetName,int _idx)
// 		{
// 			var sheet = GetSheet(_sheetName);

// 			if(_idx < 0 || _idx >= sheet.Columns.Count)
// 			{
// 				throw new IndexOutOfRangeException($"{_idx} is out of range in columnCollection. [{_sheetName}]");
// 			}

// 			var columnArray = new string[sheet.Columns.Count];

// 			for(var i=0;i<sheet.Columns.Count;i++)
// 			{
// 				columnArray[i] = sheet.Columns[_idx].ToString();
// 			}

// 			return columnArray;
// 		}

// 		/// <summary>
// 		/// Get data group in columns
// 		/// </summary>
// 		public string[][] GetColumnJaggedArray(string _sheetName,params int[] _idxArray)
// 		{
// 			var jaggedArray = new string[_idxArray.Length][];

// 			for(var i=0;i<_idxArray.Length;i++)
// 			{
// 				jaggedArray[i] = GetColumnArray(_sheetName,_idxArray[i]);
// 			}

// 			return jaggedArray;
// 		}

// 		public IEnumerable<TData> Deserialize<TData>(string _sheetName,int _startRow = 1)
// 		{
// 			var type = typeof(TData);

// 			foreach(var result in Deserialize(_sheetName,type,_startRow))
// 			{
// 				yield return (TData) result;
// 			}
// 		}

// 		public IEnumerable<object> Deserialize(string _sheetName,Type _type,int _startRow = 1)
// 		{
// 			var sheet = GetSheet(_sheetName);
// 			var propertyInfoArray = _type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
// 			var rowCollection = sheet.Rows;

// 			var lastRow = rowCollection.Count;
// 			var startRow = Math.Clamp(_startRow,0,lastRow);

// 			var headerArray = GetRowArray(_sheetName,0) ?? throw new NullReferenceException($"Header is not included in {_sheetName}");

// 			var propertyDict = new Dictionary<string,PropertyInfo>();

// 			foreach(var property in propertyInfoArray)
// 			{
// 				propertyDict[property.Name] = property;
// 			}

// 			for(var i=startRow;i<=lastRow;i++)
// 			{
// 				var cellArray = rowCollection[i].ItemArray;

// 				if(cellArray == null || cellArray.Length == 0)
// 				{
// 					continue;
// 				}

// 				var data = Activator.CreateInstance(_type);

// 				for(var j=0;j<headerArray.Length;j++)
// 				{
// 					if(j >= cellArray.Length)
// 					{
// 						break;
// 					}

// 					var cell = cellArray[j];

// 					if(cell == null)
// 					{
// 						continue;
// 					}

// 					if(propertyDict.TryGetValue(headerArray[j],out var propertyInfo) && propertyInfo.CanWrite)
// 					{
// 						try
// 						{
// 							propertyInfo.SetValue(data,ConvertData(cell.ToString(),propertyInfo.PropertyType));
// 						}
// 						catch (Exception ex)
// 						{
// 							throw new Exception($"There is a problem with the excel file. [sheet : {_sheetName} / error : {ex.Message} / location : row({i + 1}) / column({headerArray[j]})]");
// 						}
// 					}
// 				}

// 				yield return data;
// 			}
// 		}

// 		public static int IndexOf<TValue>(this IEnumerable<TValue> _sources,Predicate<TValue> _predicate)
// 		{
// 			using var enumerator = _sources.GetEnumerator();
// 			var idx = 0;

// 			while(enumerator.MoveNext())
// 			{
// 				if(_predicate(enumerator.Current))
// 				{
// 					return idx;
// 				}

// 				idx++;
// 			}

// 			return -1;
// 		}

// 		/// <summary>
// 		/// Get data group in range (x,y -> start point, w,h -> range size)
// 		/// </summary>
// 		public string[,] ConvertToArray(string _sheetName,int _x,int _y,int _width,int _height)
// 		{
// 			var sheet = GetSheet(_sheetName);
// 			var resultArray = new string[_width,_height];

// 			for(var i=_x;i<_x+_width;i++)
// 			{
// 				var cellArray = sheet.Rows[i].ItemArray;

// 				for(var j=_y;j<_y+_height;j++)
// 				{
// 					resultArray[i,j] = (cellArray == null || cellArray.Length == 0) ? string.Empty : cellArray[j].ToString();
// 				}
// 			}

// 			return resultArray;
// 		}

// 		/// <summary>
// 		/// Get title & index
// 		/// </summary>
// 		public IEnumerable<(string Title,int Index)> GetTitleGroup(string _sheetName)
// 		{
// 			var headerArray = GetRowArray(_sheetName,0) ?? throw new NullReferenceException($"Header is not included in {_sheetName}");

// 			for(var i=0;i<headerArray.Length;i++)
// 			{
// 				var header = headerArray[i];

// 				if(string.IsNullOrEmpty(header))
// 				{
// 					continue;
// 				}

// 				foreach(var keyword in KEY_WORD_ARRAY)
// 				{
// 					if(string.Equals(keyword,header))
// 					{
// 						throw new InvalidDataException($"{header} is invalid title.");
// 					}
// 				}

// 				yield return (header,i);
// 			}
// 		}

// 		private object ConvertData(string _cell,Type _type)
// 		{
// 			if(_type == typeof(string))
// 			{
// 				return _cell.Replace("\\n",Environment.NewLine);
// 			}
// 			else if(_type.IsArray)
// 			{
// 				var dataArray = _cell.Replace(" ","").TrimEnd('&',' ').Split('&');

// 				if(dataArray.Length == 0)
// 				{
// 					return Array.CreateInstance(_type.GetElementType(),0);
// 				}

// 				var type = _type.GetElementType();
// 				var resultArray = Array.CreateInstance(type,dataArray.Length);

// 				for(var i=0;i<dataArray.Length;i++)
// 				{
// 					resultArray.SetValue(ConvertToObject(dataArray[i],type),i);
// 				}

// 				return resultArray;
// 			}
// 			else
// 			{
// 				return ConvertToObject(_cell,_type);
// 			}
// 		}

// 		private object ConvertToObject(string _text,Type _type)
// 		{
// 			if(_type.IsEnum)
// 			{
// 				return Enum.Parse(_type,_text);
// 			}
// 			else if()
// 			{

// 			}
// 			else if(_type.Equals(typeof(Vector3)))
// 			{
// 				if(_text.TryToVector3(out var _result))
// 				{
// 					return _result;
// 				}
// 				else
// 				{
					
// 				}
// 			}
// 			else if(_type.IsPrimitive)
// 			{
// 				return Convert.ChangeType(_text,_type);
// 			}

// 			throw new InvalidCastException($"{_text} is not vector3.");
// 		}

// 		private static string[] KEY_WORD_ARRAY => new string[]
// 		{
// 			"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", 
// 			"class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
// 			"event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
// 			"if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
// 			"namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected",
// 			"public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", 
// 			"string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
// 			"ushort", "using", "virtual", "void", "volatile", "while",
// 		};
// 	}
// }