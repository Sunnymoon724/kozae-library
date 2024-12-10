using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using ExcelDataReader.Log;
using UnityEngine;

namespace KZLib.KZTool
{
	public class ExcelReader
	{
		private readonly DataSet _dataSet;

		public ExcelReader(string _filePath)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			Utility.IsFileExist(_filePath);

			using var stream = new FileStream(_filePath,FileMode.Open,FileAccess.Read);
			using var reader = ExcelReaderFactory.CreateReader(stream);

			_dataSet = reader.AsDataSet();
		}

		private DataTableCollection TableCollection => _dataSet != null ? _dataSet.Tables : throw new NullReferenceException("DataSet is null in ExcelReader");

		public string FirstSheetName
		{
			get
			{
				if(TableCollection.Count == 0)
				{
					throw new NullReferenceException("TableCollection count is zero");
				}

				return TableCollection[0].TableName;
			}
		}

		public IEnumerable<string> SheetNameGroup
		{
			get
			{
				var collection = TableCollection;

				for(var i=0;i<collection.Count;i++)
				{
					yield return collection[i].TableName;
				}
			}
		}

		private DataTable GetSheet(string sheetName)
		{
			var collection = TableCollection;

			return collection.Contains(sheetName) ? collection[sheetName] : throw new NullReferenceException($"The sheet '{sheetName}' does not exist in tableCollection.");
		}

		/// <summary>
		/// Get data group in row
		/// </summary>
		public string[] ExtractRowArray(string sheetName,int index)
		{
			var rowCollection = GetSheet(sheetName).Rows;

			Utility.ValidRange(sheetName,index,rowCollection.Count);

			// one row
			var rowArray = rowCollection[index].ItemArray;

			if(!IsExistRow(rowArray))
			{
				return Array.Empty<string>();
			}

			var resultArray = new string[rowArray.Length];

			for(var i=0;i<rowArray.Length;i++)
			{
				resultArray[i] = rowArray[i].ToString();
			}

			return resultArray;
		}

		/// <summary>
		/// Get data group in rows
		/// </summary>
		public string[][] ExtractRowJaggedArray(string sheetName,params int[] indexArray)
		{
			var jaggedArray = new string[indexArray.Length][];

			for(var i=0;i<indexArray.Length;i++)
			{
				jaggedArray[i] = ExtractRowArray(sheetName,indexArray[i]);
			}

			return jaggedArray;
		}

		/// <summary>
		/// Get data group in column
		/// </summary>
		public string[] ExtractColumnArray(string sheetName,int index)
		{
			var sheet = GetSheet(sheetName);

			Utility.ValidRange(sheetName,index,sheet.Columns.Count);

			var length = sheet.Rows.Count;
			var columnArray = new string[length];
			for(var i=0;i<length;i++)
			{
				columnArray[i] = sheet.Rows[i][index].ToString();
			}

			return columnArray;
		}

		/// <summary>
		/// Get data group in columns
		/// </summary>
		public string[][] ExtractColumnJaggedArray(string sheetName,params int[] indexArray)
		{
			var jaggedArray = new string[indexArray.Length][];

			for(var i=0;i<indexArray.Length;i++)
			{
				jaggedArray[i] = ExtractColumnArray(sheetName,indexArray[i]);
			}

			return jaggedArray;
		}

		public IEnumerable<TData> DeserializeGroup<TData>(string sheetName,bool includeBlank,int startRow = 1)
		{
			var dataType = typeof(TData);

			foreach(var result in DeserializeGroup(sheetName,dataType,includeBlank,startRow))
			{
				yield return (TData) result;
			}
		}

		public IEnumerable<object> DeserializeGroup(string sheetName,Type dataType,bool includeBlank,int startRow = 1)
		{
			var sheet = GetSheet(sheetName);
			var rowCollection = sheet.Rows;

			var lastRow = rowCollection.Count;
			startRow = Math.Clamp(startRow,0,lastRow);

			var schemeArray = ExtractRowArray(sheetName,0) ?? throw new NullReferenceException($"Scheme is not included in {sheetName}");

			// n -> last (get row)
			for(var i=startRow;i<lastRow;i++)
			{
				var cellArray = rowCollection[i].ItemArray;

				if(!IsExistRow(cellArray))
				{
					continue;
				}

				yield return Deserialize(schemeArray,dataType,cellArray,includeBlank);
			}
		}

		public TData Deserialize<TData>(string[] schemeArray,object[] cellArray,bool includeBlank)
		{
			var dataType = typeof(TData);

			return (TData) Deserialize(schemeArray,dataType,cellArray,includeBlank);
		}

		public object Deserialize(string[] schemeArray,Type dataType,object[] cellArray,bool includeBlank)
		{
			var instance = Activator.CreateInstance(dataType);
			var indexList = new List<int>();

			foreach(var propertyInfo in dataType.GetProperties())
			{
				indexList.Clear();
				var propertyType = propertyInfo.PropertyType;
				var propertyName = propertyInfo.Name;

				for(var i=0;i<schemeArray.Length;i++)
				{
					if(string.Equals(schemeArray[i],propertyName,StringComparison.Ordinal))
					{
						indexList.Add(i);
					}
				}

				if(indexList.Count < 1)
				{
					throw new ArgumentNullException($"{propertyName} is not include in {string.Join("/",schemeArray)}");
				}

				if(propertyType.IsArray)
				{
					var elementType = propertyType.GetElementType();
					var resultArray = Array.CreateInstance(elementType,indexList.Count);

					for(var i=0;i<indexList.Count;i++)
					{
						var index = indexList[i];

						if(index <= cellArray.Length)
						{
							continue;
						}

						resultArray.SetValue(GetCell(cellArray,index,propertyType,includeBlank),i);
					}

					propertyInfo.SetValue(instance,resultArray);
				}
				else
				{
					propertyInfo.SetValue(instance,GetCell(cellArray,indexList[0],propertyType,includeBlank));
				}
			}

			return instance;
		}

		private object GetCell(object[] cellArray,int index,Type dataType,bool includeBlank)
		{
			if(0 <= index && index < cellArray.Length)
			{
				var cellText = cellArray[index].ToString();

				if(!includeBlank && string.IsNullOrEmpty(cellText))
				{
					throw new ArgumentException($"cell is empty in {index}. [type : {dataType}]");
				}

				return ConvertToObject(cellText,dataType);
			}
			else
			{
				throw new IndexOutOfRangeException($"{cellArray.Length} < {index}");
			}
		}

		/// <summary>
		/// Get data group in range (x,y -> start point, w,h -> range size)
		/// </summary>
		public string[,] ConvertToArray(string sheetName,int x,int y,int width,int height)
		{
			var sheet = GetSheet(sheetName);
			var resultArray = new string[width,height];

			for(var i=x;i<x+width;i++)
			{
				var cellArray = sheet.Rows[i].ItemArray;

				for(var j=y;j<y+height;j++)
				{
					resultArray[i,j] = (cellArray == null || cellArray.Length == 0) ? string.Empty : cellArray[j].ToString();
				}
			}

			return resultArray;
		}

		/// <summary>
		/// Get title & index
		/// </summary>
		public IEnumerable<(string Title,int Index)> GetSchemeGroup(string sheetName)
		{
			var schemeArray = ExtractRowArray(sheetName,0) ?? throw new NullReferenceException($"Scheme is not included in {sheetName}");

			for(var i=0;i<schemeArray.Length;i++)
			{
				var scheme = schemeArray[i];

				if(string.IsNullOrEmpty(scheme))
				{
					continue;
				}

				foreach(var keyword in KEY_WORD_ARRAY)
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
		/// empty or #start -> not exist
		/// </summary>
		private bool IsExistRow(object[] cellArray)
		{
			if(cellArray == null || cellArray.Length == 0)
			{
				return false;
			}

			var header = cellArray[0].ToString();

			return !header.StartsWith("#") && !string.IsNullOrEmpty(header);
		}

		private object ConvertToObject(string cellText,Type dataType)
		{
			if(dataType == typeof(string))
			{
				return cellText.Replace("\\n",Environment.NewLine);
			}
			else if(dataType.IsEnum)
			{
				if(Enum.TryParse(dataType,cellText,out var enumValue))
				{
					return enumValue;
				}
				else
				{
					throw new InvalidCastException($"{cellText} is not include in {dataType.Name}.");
				}
			}
			else if(dataType.Equals(typeof(Vector2)))
			{
				var vectorArray = cellText.Trim('(',')').Split(',');

				if(vectorArray.Length != 2)
				{
					throw new InvalidCastException($"{cellText} is not vector2.");
				}

				return new Vector2(float.Parse(vectorArray[0]),float.Parse(vectorArray[1]));
			}
			else if(dataType.Equals(typeof(Vector3)))
			{
				var vectorArray = cellText.Trim('(',')').Split(',');

				if(vectorArray.Length != 3)
				{
					throw new InvalidCastException($"{cellText} is not vector3.");
				}

				return new Vector3(float.Parse(vectorArray[0]),float.Parse(vectorArray[1]),float.Parse(vectorArray[2]));
			}
			else if(dataType.IsPrimitive)
			{
				return Convert.ChangeType(cellText,dataType);
			}

			throw new NotSupportedException($"{cellText} is not supported.");
		}

		public DataTable MergeSheet(DataTable destination,DataTable source,int start = 1)
		{
			for(var i=source.Columns.Count-destination.Columns.Count;i>=0;i--)
			{
				destination.Columns.Add();
			}

			for(var i=start;i<source.Rows.Count;i++)
			{
				destination.Rows.Add(source.Rows[i].ItemArray);
			}

			return destination;
		}

		private string[] KEY_WORD_ARRAY => new string[]
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