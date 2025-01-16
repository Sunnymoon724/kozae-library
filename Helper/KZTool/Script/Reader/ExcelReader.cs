using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using UnityEngine;

namespace KZLib.KZTool
{
	public class ExcelReader
	{
		private readonly DataSet m_dataSet;

		public ExcelReader(string filePath)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			if(!File.Exists(filePath))
			{
				throw new NullReferenceException($"{filePath} is not exist");
			}

			using var stream = new FileStream(filePath,FileMode.Open,FileAccess.Read);
			using var reader = ExcelReaderFactory.CreateReader(stream);

			m_dataSet = reader.AsDataSet();
		}

		private DataTableCollection TableCollection => m_dataSet != null ? m_dataSet.Tables : throw new NullReferenceException("DataSet is null in ExcelReader");

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

		public string FindSheetName(Func<string,bool> condition)
		{
			var collection = TableCollection;

			for(var i=0;i<collection.Count;i++)
			{
				var sheetName = collection[i].TableName;

				if(condition.Invoke(sheetName))
				{
					return sheetName;
				}
			}

			throw new ArgumentNullException($"SheetName is not exist in condition");
		}

		private DataTable GetSheet(string sheetName)
		{
			var collection = TableCollection;

			return collection.Contains(sheetName) ? collection[sheetName] : throw new NullReferenceException($"The sheet '{sheetName}' does not exist in tableCollection.");
		}

		private string[] ConvertRowToStringArray(object[] rowArray)
		{
			if(!IsExistRow(rowArray))
			{
				return Array.Empty<string>();
			}

			var cellArray = new string[rowArray.Length];

			for(var i=0;i<rowArray.Length;i++)
			{
				cellArray[i] = rowArray[i].ToString();
			}

			return cellArray;
		}

		/// <summary>
		/// Get data group in row
		/// </summary>
		public string[] ExtractRowArray(string sheetName,int index)
		{
			var rowCollection = GetSheet(sheetName).Rows;

			IsValidRange(sheetName,index,rowCollection.Count);

			return ConvertRowToStringArray(rowCollection[index].ItemArray);
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

			IsValidRange(sheetName,index,sheet.Columns.Count);

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
				var cellArray = ConvertRowToStringArray(rowCollection[i].ItemArray);

				if(cellArray.Length == 0)
				{
					continue;
				}

				yield return Deserialize(schemeArray,dataType,cellArray,includeBlank);
			}
		}

		public TData Deserialize<TData>(string[] schemeArray,string[] cellArray,bool includeBlank)
		{
			var dataType = typeof(TData);

			return (TData) Deserialize(schemeArray,dataType,cellArray,includeBlank);
		}

		public object Deserialize(string[] schemeArray,Type dataType,string[] cellArray,bool includeBlank)
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

						resultArray.SetValue(ConvertCell(cellArray[index],index,propertyType,includeBlank),i);
					}

					propertyInfo.SetValue(instance,resultArray);
				}
				else
				{
					var index = indexList[0];

					propertyInfo.SetValue(instance,ConvertCell(cellArray[index],index,propertyType,includeBlank));
				}
			}

			return instance;
		}

		private object ConvertCell(string cell,int index,Type targetType,bool includeBlank)
		{
			if (!includeBlank && string.IsNullOrEmpty(cell))
			{
				throw new ArgumentException($"cell is empty in {index}. [type : {targetType}]");
			}

			return ConvertToObject(cell,targetType);
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

		private object ConvertToObject(string cellText,Type targetType)
		{
			if(targetType == typeof(string))
			{
				return cellText.Replace("\\n",Environment.NewLine);
			}

			if(targetType.IsEnum)
			{
				if(Enum.TryParse(targetType,cellText,out var enumValue))
				{
					return enumValue;
				}
				else
				{
					throw new InvalidCastException($"{cellText} is not include in {targetType.Name}.");
				}
			}

			if(targetType == typeof(Vector2) || targetType == typeof(Vector3))
			{
				var vectorArray = cellText.Trim('(',')').Split(',');

				if(targetType == typeof(Vector2))
				{
					return new Vector2(float.Parse(vectorArray[0]),float.Parse(vectorArray[1]));
				}
				else
				{
					return new Vector3(float.Parse(vectorArray[0]),float.Parse(vectorArray[1]),float.Parse(vectorArray[2]));
				}
			}

			if(targetType.IsPrimitive)
			{
				return Convert.ChangeType(cellText, targetType);
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

		private void IsValidRange(string sheetName,int index,int count)
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