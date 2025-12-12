using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using KZConsole.KZUtility;
using KZLib.KZTool;
using KZLib.KZUtility;

namespace KZConsole
{
	public class ProtoBuilder(List<string> protoFilePathList, string projectFolderPath)
	{
		private struct EnumScheme
		{
			public string Key { get; set; }
			public string Value { get; set; }
			public string Comment { get; set; }
		}

		private readonly List<string> m_protoFilePathList = protoFilePathList;
		private readonly string m_enumExcelFilePath = FileUtility.FindFilePath(protoFilePathList,"Enum");
		private readonly string m_projectFolderPath = projectFolderPath;
		private readonly string m_newLine = Environment.NewLine;

		public void GenerateAllProtoCode()
		{
			Console.WriteLine("Generate all proto code.");
			Console.WriteLine("-Copy default proto code.");
			
			Assembly assembly = Assembly.GetExecutingAssembly();

			_CopyDefaultProtoCode(assembly);

			var templateFileDict = CommonUtility.ReadEmbeddedResourcesFromExtension(assembly,".txt");

			Console.WriteLine("-Generate enum code.");
			
			if(templateFileDict.TryGetValue("EnumTemplate.txt",out var enumTemplate))
			{
				_GenerateEnumCode(enumTemplate);
			}
			else
			{
				throw new FileNotFoundException("Enum template file not found.");
			}

			Console.WriteLine("-Generate proto code.");
			
			if(templateFileDict.TryGetValue("ProtoTemplate.txt",out var protoTemplate))
			{
				_GenerateProtoCode(protoTemplate);
			}
			else
			{
				throw new FileNotFoundException("Proto template file not found.");
			}
		}

		private void _CopyDefaultProtoCode(Assembly assembly)
		{
			var protoFileDict = CommonUtility.ReadEmbeddedResourcesFromExtension(assembly,".cs");

			foreach(var pair in protoFileDict)
			{
				_WriteTextToFile(pair.Key,pair.Value);
			}
		}

		private void _GenerateEnumCode(string templateFile)
		{
			var enumBuilder = new StringBuilder();
			var excelReader = new ExcelReader(m_enumExcelFilePath);
			var collection = excelReader.SheetNameGroup;
			var currentIndex = 0;

			foreach(var sheetName in collection)
			{
				enumBuilder.Append($"\tpublic enum {sheetName}{m_newLine}");
				enumBuilder.Append($"\t{{{m_newLine}");

				var index = -1;

				foreach(var scheme in excelReader.DeserializeGroup<EnumScheme>(sheetName))
				{
					index = int.TryParse(scheme.Value,out var number) ? number : ++index;

					enumBuilder.Append($"\t\t{scheme.Key} = {index}, // {scheme.Comment}{m_newLine}");
				}

				currentIndex++;

				if(currentIndex < collection.Count)
				{
					enumBuilder.Append($"\t}}{m_newLine}{m_newLine}");
				}
				else
				{
					enumBuilder.Append($"\t}}{m_newLine}");
				}
			}

			if(enumBuilder.Length <= 0)
			{
				return;
			}

			var enumCode = enumBuilder.ToString();
			var enumFile = templateFile;

			enumFile = enumFile.Replace("$Enums",enumCode);

			_WriteTextToFile("Enum.cs",enumFile);
		}

		private void _GenerateProtoCode(string templateFile)
		{
			var excludeFileNameList = new List<string>
			{
				"Enum",
				"Branch",

				"Color",
				"Motion",
				"NetworkError",
			};

			for(int i=0;i<m_protoFilePathList.Count;i++)
			{
				var protoFilePath = m_protoFilePathList[i];
				var fileName = FileUtility.GetOnlyName(protoFilePath);

				if(excludeFileNameList.Contains(fileName))
				{
					continue;
				}

				var excelReader = new ExcelReader(protoFilePath);
				var sheetNameArray = excelReader.FindSheetNameArray(x=>x.StartsWith('+'));
				var nameCount = sheetNameArray.Length;

				if(nameCount < 1)
				{
					Console.WriteLine($"Warning : {fileName} is not include +Sheet");

					continue;
				}

				var mainClassCode = string.Empty;
				var subClassCode = string.Empty;

				mainClassCode = _GenerateClassTemplate(excelReader,sheetNameArray[0],true,protoFilePath);

				if(nameCount != 1)
				{
					var classBuilder = new StringBuilder();

					for(var j=1;j<nameCount;j++)
					{
						classBuilder.Append($"{m_newLine}{m_newLine}{_GenerateClassTemplate(excelReader,sheetNameArray[j],false,protoFilePath)}");
					}

					subClassCode = classBuilder.ToString();
				}

				var protoFile = templateFile;

				protoFile = protoFile.Replace("$MainClass",mainClassCode);
				protoFile = protoFile.Replace("$SubClass",subClassCode);

				_WriteTextToFile($"{fileName}Proto.cs",protoFile);
			}
		}

		private string _GenerateClassTemplate(ExcelReader excelReader,string sheetName,bool isMain,string filePath)
		{
			var name = sheetName.TrimStart('+');
			var className = isMain ? $"{name}Proto : IProto" : name;

			var propertyCode = _GeneratePropertyCode(excelReader,sheetName);

			if(string.IsNullOrEmpty(propertyCode))
			{
				throw new NullReferenceException($"Generate failed in {sheetName}. [{filePath}]");
			}

			var classBuilder = new StringBuilder();

			classBuilder.Append($"\t[MemoryPackable]{m_newLine}");
			classBuilder.Append($"\tpublic partial class {className}{m_newLine}");
			classBuilder.Append($"\t{{{m_newLine}");
			classBuilder.Append($"{propertyCode}{m_newLine}");
			classBuilder.Append($"\t}}");

			return classBuilder.ToString();
		}

		private string _GeneratePropertyCode(ExcelReader excelReader,string sheetName)
		{
			var propertyList = new List<string>();
			var propertyBuilder = new StringBuilder();
			var protoJaggedArray = excelReader.MergeCellArrayInRows(sheetName,[0,1]);
			var schemeArray = protoJaggedArray[0];
			var schemeLength = schemeArray.Length;
			var keyIndex = 0;

			for(int i=0;i<schemeLength;i++)
			{
				var property = schemeArray[i].Split(':')[0];

				// remove overlap
				if(string.IsNullOrEmpty(property) || property.StartsWith('%') || propertyList.Contains(property))
				{
					continue;
				}

				var type = protoJaggedArray[1][i];

				propertyBuilder.Append($"\t\t[MemoryPackOrder({keyIndex++})]{m_newLine}");
				propertyBuilder.Append($"\t\tpublic {type} {property} {{ get; init; }}{m_newLine}");

				propertyList.Add(property);
			}

			if(propertyBuilder.Length <= 0)
			{
				return string.Empty;
			}

			propertyBuilder.Length -= m_newLine.Length;

			return propertyBuilder.ToString();
		}

		private void _WriteTextToFile(string fileName,string text)
		{
			var filePath = Path.Combine(m_projectFolderPath,fileName);

			FileUtility.WriteTextToFile(filePath,text);
		}
	}
}