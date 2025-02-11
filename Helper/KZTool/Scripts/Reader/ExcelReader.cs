// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.IO;
// using System.Text;
// using ExcelDataReader;
// using UnityEngine;

// namespace KZLib.KZTool
// {
// 	public class ExcelReader
// 	{
// 		private DataSet ExcelDataSet { get; }
// 		public string FilePath { get; }

// 		private readonly Dictionary<string,DataTable> m_sheetDict = new Dictionary<string,DataTable>();

// 		public ExcelReader(string filePath)
// 		{
// 			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// 			if(!File.Exists(filePath))
// 			{
// 				throw new NullReferenceException($"{filePath} is not exist");
// 			}

// 			using var stream = new FileStream(filePath,FileMode.Open,FileAccess.Read);
// 			using var reader = ExcelReaderFactory.CreateReader(stream);

// 			ExcelDataSet = reader.AsDataSet();
// 			FilePath = filePath;

// 			var collection = ExcelDataSet.Tables;

// 			for(var i=0;i<collection.Count;i++)
// 			{
// 				var sheet = collection[i];

// 				m_sheetDict.Add(sheet.TableName,sheet);
// 			}
// 		}

// 		private void IsValidExcel()
// 		{
// 			if(ExcelDataSet == null)
// 			{
// 				throw new NullReferenceException("ExcelReader is not exist");
// 			}

// 			if(m_sheetDict.Count < 1)
// 			{
// 				throw new NullReferenceException("Sheet is not exist");
// 			}
// 		}

// 		public IEnumerable<string> SheetNameGroup
// 		{
// 			get
// 			{
// 				IsValidExcel();

// 				foreach(var key in m_sheetDict.Keys)
// 				{
// 					yield return key;
// 				}
// 			}
// 		}

// 		public string FindSheetName(Func<string,bool> condition)
// 		{
// 			IsValidExcel();

// 			foreach(var key in m_sheetDict.Keys)
// 			{
// 				if(condition.Invoke(key))
// 				{
// 					return key;
// 				}
// 			}

// 			throw new ArgumentNullException("SheetName is not exist in condition");
// 		}

// 		public List<string> FindSheetNameList(Func<string,bool> condition)
// 		{
// 			IsValidExcel();

// 			var nameList = new List<string>(m_sheetDict.Count);

// 			foreach(var key in m_sheetDict.Keys)
// 			{
// 				if(condition.Invoke(key))
// 				{
// 					nameList.Add(key);
// 				}
// 			}

// 			if(nameList.Count < 1)
// 			{
// 				throw new ArgumentNullException("SheetName is not exist in condition");
// 			}

// 			return nameList;
// 		}

// 		public bool IsExistSheetName(string sheetName)
// 		{
// 			IsValidExcel();

// 			return m_sheetDict.ContainsKey(sheetName);
// 		}

// 		private DataTable GetSheet(string sheetName)
// 		{
// 			return m_sheetDict.TryGetValue(sheetName,out var sheet) ? sheet : throw new NullReferenceException($"The sheet '{sheetName}' does not exist in tableCollection.");
// 		}

// 		public int GetRowSize(string sheetName)
// 		{
// 			return GetSheet(sheetName).Rows.Count;
// 		}

// 		/// <summary>
// 		/// Get data group in row
// 		/// </summary>
// 		public object[] ExtractRowArray(string sheetName,int index)
// 		{
// 			var rowCollection = GetSheet(sheetName).Rows;

// 			IsValidRange(sheetName,index,rowCollection.Count);

// 			return rowCollection[index].ItemArray;
// 		}

// 		/// <summary>
// 		/// Get data group in row
// 		/// </summary>
// 		public object[] ExtractRowArray(string sheetName,Func<object,bool> condition)
// 		{
// 			var rowCollection = GetSheet(sheetName).Rows;
// 			var index = -1;

// 			for(var i=0;i<rowCollection.Count;i++)
// 			{
// 				if(condition.Invoke(rowCollection[i]))
// 				{
// 					index = i;
// 					break;
// 				}
// 			}

// 			return ExtractRowArray(sheetName,index);
// 		}

// 		/// <summary>
// 		/// Get data group in rows
// 		/// </summary>
// 		public object[][] ExtractRowJaggedArray(string sheetName,params int[] indexArray)
// 		{
// 			var jaggedArray = new object[indexArray.Length][];

// 			for(var i=0;i<indexArray.Length;i++)
// 			{
// 				jaggedArray[i] = ExtractRowArray(sheetName,indexArray[i]);
// 			}

// 			return jaggedArray;
// 		}

// 		/// <summary>
// 		/// Get data group in row
// 		/// </summary>
// 		public object[][] ExtractRowJaggedArray(string sheetName,Func<object,bool> condition)
// 		{
// 			var rowCollection = GetSheet(sheetName).Rows;
// 			var indexList = new List<int>();

// 			for(var i=0;i<rowCollection.Count;i++)
// 			{
// 				if(condition.Invoke(rowCollection[i]))
// 				{
// 					indexList.Add(i);
// 				}
// 			}

// 			return ExtractRowJaggedArray(sheetName,indexList.ToArray());
// 		}

// 		/// <summary>
// 		/// Get data group in column
// 		/// </summary>
// 		public object[] ExtractColumnArray(string sheetName,int index)
// 		{
// 			var sheet = GetSheet(sheetName);

// 			IsValidRange(sheetName,index,sheet.Columns.Count);

// 			var length = sheet.Rows.Count;
// 			var columnArray = new object[length];

// 			for(var i=0;i<length;i++)
// 			{
// 				columnArray[i] = sheet.Rows[i][index];
// 			}

// 			return columnArray;
// 		}

// 		/// <summary>
// 		/// Get data group in columns
// 		/// </summary>
// 		public object[][] ExtractColumnJaggedArray(string sheetName,params int[] indexArray)
// 		{
// 			var jaggedArray = new object[indexArray.Length][];

// 			for(var i=0;i<indexArray.Length;i++)
// 			{
// 				jaggedArray[i] = ExtractColumnArray(sheetName,indexArray[i]);
// 			}

// 			return jaggedArray;
// 		}

// 		public IEnumerable<TData> DeserializeGroup<TData>(string sheetName,int startRow = 1)
// 		{
// 			var dataType = typeof(TData);

// 			foreach(var result in DeserializeGroup(sheetName,dataType,startRow))
// 			{
// 				yield return (TData) result;
// 			}
// 		}

// 		public IEnumerable<object> DeserializeGroup(string sheetName,Type dataType,int startRow = 1)
// 		{
// 			var sheet = GetSheet(sheetName);
// 			var rowCollection = sheet.Rows;

// 			var lastRow = rowCollection.Count;
// 			startRow = Math.Clamp(startRow,0,lastRow);

// 			var schemeArray = ExtractRowArray(sheetName,0) ?? throw new NullReferenceException($"Scheme is not included in {sheetName}");

// 			// n -> last (get row)
// 			for(var i=startRow;i<lastRow;i++)
// 			{
// 				var cellArray = rowCollection[i].ItemArray;

// 				if(cellArray.Length == 0)
// 				{
// 					continue;
// 				}

// 				yield return Deserialize(schemeArray,dataType,cellArray,i);
// 			}
// 		}

// 		public TData Deserialize<TData>(object[] schemeArray,object[] cellArray,int line)
// 		{
// 			var dataType = typeof(TData);

// 			return (TData) Deserialize(schemeArray,dataType,cellArray,line);
// 		}

// 		public object Deserialize(object[] schemeArray,Type dataType,object[] cellArray,int line)
// 		{
// 			var instance = Activator.CreateInstance(dataType);
// 			var indexList = new List<int>();

// 			foreach(var propertyInfo in dataType.GetProperties())
// 			{
// 				indexList.Clear();
// 				var propertyType = propertyInfo.PropertyType;
// 				var propertyName = propertyInfo.Name;

// 				for(var i=0;i<schemeArray.Length;i++)
// 				{
// 					var scheme = schemeArray[i].ToString();

// 					if(string.Equals(scheme,propertyName,StringComparison.Ordinal))
// 					{
// 						indexList.Add(i);
// 					}
// 				}

// 				if(indexList.Count < 1)
// 				{
// 					throw new ArgumentNullException($"{propertyName} is not include in {string.Join("/",schemeArray)}");
// 				}

// 				if(propertyType.IsArray)
// 				{
// 					var elementType = propertyType.GetElementType();
// 					var resultArray = Array.CreateInstance(elementType,indexList.Count);

// 					for(var i=0;i<indexList.Count;i++)
// 					{
// 						var index = indexList[i];

// 						if(index <= cellArray.Length)
// 						{
// 							continue;
// 						}

// 						var cell = cellArray[index].ToString();

// 						resultArray.SetValue(ConvertCell(cell,line,propertyType),i);
// 					}

// 					propertyInfo.SetValue(instance,resultArray);
// 				}
// 				else
// 				{
// 					var index = indexList[0];
// 					var cell = cellArray[index].ToString();

// 					propertyInfo.SetValue(instance,ConvertCell(cell,line,propertyType));
// 				}
// 			}

// 			return instance;
// 		}

// 		private object ConvertCell(string cell,int line,Type targetType)
// 		{
// 			if(targetType == typeof(string))
// 			{
// 				return cell.Replace("\\n",Environment.NewLine);
// 			}

// 			if(targetType.IsEnum)
// 			{
// 				if(Enum.TryParse(targetType,cell,out var enumValue))
// 				{
// 					return enumValue;
// 				}
// 				else
// 				{
// 					throw new InvalidCastException($"{cell} is not include in {targetType.Name}. [line : {line} / type : {targetType}]");
// 				}
// 			}

// 			if(targetType == typeof(Vector2) || targetType == typeof(Vector3))
// 			{
// 				var vectorArray = cell.Trim('(', ')').Split(',');

// 				try
// 				{
// 					var valueArray = Array.ConvertAll(vectorArray,float.Parse);

// 					if(targetType == typeof(Vector2) && valueArray.Length == 2)
// 					{
// 						return new Vector2(valueArray[0],valueArray[1]);
// 					}

// 					if(targetType == typeof(Vector3) && valueArray.Length == 3)
// 					{
// 						return new Vector3(valueArray[0],valueArray[1],valueArray[2]);
// 					}

// 					throw new FormatException($"{cell} is not a valid {targetType.Name}. [line: {line} / type: {targetType}]");
// 				}
// 				catch(Exception exception)
// 				{
// 					throw new FormatException($"{exception.Message} in {cell}. [line: {line} / type: {targetType}]");
// 				}
// 			}

// 			if(targetType.IsPrimitive)
// 			{
// 				return Convert.ChangeType(cell,targetType);
// 			}

// 			return null!;
// 		}

// 		/// <summary>
// 		/// Get data group in range (x,y -> start point, w,h -> range size)
// 		/// </summary>
// 		public string[,] ConvertToArray(string sheetName,int x,int y,int width,int height)
// 		{
// 			var sheet = GetSheet(sheetName);
// 			var resultArray = new string[width,height];

// 			for(var i=x;i<x+width;i++)
// 			{
// 				var cellArray = sheet.Rows[i].ItemArray;

// 				for(var j=y;j<y+height;j++)
// 				{
// 					resultArray[i,j] = (cellArray == null || cellArray.Length == 0) ? string.Empty : cellArray[j].ToString();
// 				}
// 			}

// 			return resultArray;
// 		}

// 		/// <summary>
// 		/// Get title & index
// 		/// </summary>
// 		public IEnumerable<(string Title,int Index)> GetSchemeGroup(string sheetName)
// 		{
// 			var schemeArray = ExtractRowArray(sheetName,0) ?? throw new NullReferenceException($"Scheme is not included in {sheetName}");

// 			for(var i=0;i<schemeArray.Length;i++)
// 			{
// 				var scheme = schemeArray[i].ToString();

// 				if(string.IsNullOrEmpty(scheme))
// 				{
// 					continue;
// 				}

// 				foreach(var keyword in s_keyword_array)
// 				{
// 					if(string.Equals(keyword,scheme))
// 					{
// 						throw new InvalidDataException($"{scheme} is invalid title.");
// 					}
// 				}

// 				yield return (scheme,i);
// 			}
// 		}

// 		/// <summary>
// 		/// empty or #start -> not exist
// 		/// </summary>
// 		private bool IsExistRow(object[] cellArray)
// 		{
// 			if(cellArray == null || cellArray.Length == 0)
// 			{
// 				return false;
// 			}

// 			var header = cellArray[0].ToString();

// 			return !header.StartsWith("#") && !string.IsNullOrEmpty(header);
// 		}

// 		public DataTable MergeSheet(DataTable destination,DataTable source,int start = 1)
// 		{
// 			for(var i=source.Columns.Count-destination.Columns.Count;i>=0;i--)
// 			{
// 				destination.Columns.Add();
// 			}

// 			for(var i=start;i<source.Rows.Count;i++)
// 			{
// 				destination.Rows.Add(source.Rows[i].ItemArray);
// 			}

// 			return destination;
// 		}

// 		private void IsValidRange(string sheetName,int index,int count)
// 		{
// 			if(index < 0 || index >= count)
// 			{
// 				throw new IndexOutOfRangeException($"{index} is out of range in {sheetName}");
// 			}
// 		}

// 		private static readonly string[] s_keyword_array = new string[]
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