using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using KZConsole.Utilities;
using KZLib.ToolKits;

namespace KZConsole
{
	public class ProtoBuilder(List<string> protoFilePathList,string projectFolderPath)
	{
		private struct EnumScheme
		{
			public string Key { get; set; }
			public string Value { get; set; }
			public string Comment { get; set; }
		}

		private readonly List<string> m_protoFilePathList = protoFilePathList;
		private readonly string m_projectFolderPath = projectFolderPath;
		private readonly string m_newLine = Environment.NewLine;

		public void GenerateAllProtoCode()
		{
			KZCommonKit.WriteLog("Generate all proto code.",LogType.Info);

			var assembly = Assembly.GetExecutingAssembly();
			var embeddedResourceDict = KZCommonKit.ReadEmbeddedResourcesFromExtensions(assembly,".cs",".txt");

			KZCommonKit.WriteLog("-Copy default proto code.",LogType.Info);

			var excludeProtoNameHashSet = _CopyDefaultProtoCode(embeddedResourceDict);

			KZCommonKit.WriteLog("-Generate enum code.",LogType.Info);

			if(embeddedResourceDict.TryGetValue("EnumTemplate.txt",out var enumTemplate))
			{
				_GenerateEnumCode(enumTemplate);
			}
			else
			{
				throw new FileNotFoundException("Enum template file not found.");
			}

			KZCommonKit.WriteLog("-Generate proto code.",LogType.Info);

			if(embeddedResourceDict.TryGetValue("ProtoTemplate.txt",out var protoTemplate))
			{
				_GenerateProtoCode(protoTemplate,excludeProtoNameHashSet);
			}
			else
			{
				throw new FileNotFoundException("Proto template file not found.");
			}
		}

		private HashSet<string> _CopyDefaultProtoCode(Dictionary<string,string> embeddedResourceDict)
		{
			var excludeProtoNameHashSet = new HashSet<string>(ProtoGlobal.ExcludeFileNameHashSet);

			foreach(var pair in embeddedResourceDict)
			{
				if(!pair.Key.EndsWith(".cs"))
				{
					continue;
				}

				KZFileKit.WriteTextToFile(m_projectFolderPath,pair.Key,pair.Value);

				if(KZProtoKit.TryGetEmbeddedProtoName(pair.Key,out var excelName))
				{
					excludeProtoNameHashSet.Add(excelName);
				}
			}

			return excludeProtoNameHashSet;
		}

		private void _GenerateEnumCode(string templateFile)
		{
			var enumExcelFilePath = KZFileKit.FindFilePath(m_protoFilePathList,"Enum");
			
			if(string.IsNullOrEmpty(enumExcelFilePath))
			{
				KZCommonKit.WriteLog("Warning : Enum excel file not found.",LogType.Warning);

				return;
			}

			var enumBlockList = new List<string>();
			var excelReader = new ExcelReader(enumExcelFilePath);

			foreach(var sheetName in excelReader.SheetNameGroup)
			{
				var textList = new List<string>
                {
                    $"\tpublic enum {sheetName}",
                    "\t{"
                };

				var index = -1;

				foreach(var scheme in excelReader.DeserializeGroup<EnumScheme>(sheetName))
				{
					index = int.TryParse(scheme.Value,out var number) ? number : ++index;

					textList.Add($"\t\t{scheme.Key} = {index}, // {scheme.Comment}");
				}

				textList.Add("\t}");

				enumBlockList.Add(string.Join(m_newLine,textList));
			}

			if(enumBlockList.Count == 0)
			{
				return;
			}

			var enumCode = string.Join($"{m_newLine}{m_newLine}",enumBlockList);
			var enumFile = templateFile.Replace("$Enum",enumCode);

			KZFileKit.WriteTextToFile(m_projectFolderPath,"Enum.cs",enumFile);
		}

		private void _GenerateProtoCode(string templateFile,HashSet<string> excludeProtoNameHashSet)
		{
			for(int i=0;i<m_protoFilePathList.Count;i++)
			{
				var protoFilePath = m_protoFilePathList[i];
				var fileName = KZFileKit.GetOnlyName(protoFilePath);

				if(excludeProtoNameHashSet.Contains(fileName))
				{
					continue;
				}

				var excelReader = new ExcelReader(protoFilePath);
				var sheetNameArray = excelReader.FindSheetNameArrayByPrefix(ProtoGlobal.SheetPrefix);
				var nameCount = sheetNameArray.Length;

				if(nameCount < 1)
				{
					KZCommonKit.WriteLog($"Warning : {fileName} is not include +Sheet", LogType.Warning);

					continue;
				}

				var mainClassCode = _GenerateClassTemplate(excelReader,sheetNameArray[ProtoGlobal.MainSheetIndex],true,protoFilePath);
				var subClassList = new List<string>();

				for(var j=ProtoGlobal.SubSheetStartIndex;j<nameCount;j++)
				{
					subClassList.Add(_GenerateClassTemplate(excelReader,sheetNameArray[j],false,protoFilePath));
				}

				var subClassCode = subClassList.Count == 0 ? string.Empty : string.Join($"{m_newLine}{m_newLine}",subClassList);

				var protoFile = templateFile;

				protoFile = protoFile.Replace("$MainClass",mainClassCode);
				protoFile = protoFile.Replace("$SubClass",subClassCode);

				KZFileKit.WriteTextToFile(m_projectFolderPath,$"{fileName}Proto.cs",protoFile);
			}
		}

		private string _GenerateClassTemplate(ExcelReader excelReader,string sheetName,bool isMain,string filePath)
		{
			var name = KZProtoKit.TrimProtoName(sheetName);
			var className = isMain ? $"{name}Proto : IProto" : name;

			var propertyCode = _GeneratePropertyCode(excelReader,sheetName);

			if(string.IsNullOrEmpty(propertyCode))
			{
				throw new InvalidOperationException($"Generate failed in {sheetName}. [{filePath}]");
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
			var propertyNameList = new List<string>();
			var textList = new List<string>();
			var protoJaggedArray = excelReader.MergeCellArrayInRows(sheetName,[ProtoGlobal.SchemeRowIndex,ProtoGlobal.TypeRowIndex]);
			var schemeArray = protoJaggedArray[ProtoGlobal.SchemeRowIndex];
			var typeArray = protoJaggedArray[ProtoGlobal.TypeRowIndex];
			var schemeLength = schemeArray.Length;
			var keyIndex = 0;

			for(int i=0;i<schemeLength;i++)
			{
				var property = schemeArray[i].Replace(":pk","");

				// remove overlap
				if(string.IsNullOrEmpty(property) || property.StartsWith('%') || propertyNameList.Contains(property))
				{
					continue;
				}

				var type = typeArray[i].Split(':')[0];

				textList.Add($"\t\t[MemoryPackOrder({keyIndex++})]");
				textList.Add($"\t\tpublic {type} {property} {{ get; init; }}");

				propertyNameList.Add(property);
			}

			if(textList.Count == 0)
			{
				return string.Empty;
			}

			return string.Join(m_newLine,textList);
		}
	}
}