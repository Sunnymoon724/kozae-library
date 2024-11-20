using NX.Proto;
using NX.Proto.Div6;
using NX.Util;
using NX.Util.Proto;
using Scriban;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace GSAGenerator
{
    public class ProtoClassGenerator
    {

        public bool LoadTemplate(string templatePath)
        {
            _templatePath = templatePath;
            return true;
        }

        public bool TryGenerateWithExcelDir(string xlsxFileDir, string csFileDir)
        {
            var xlsxFileArr = Directory.GetFiles(xlsxFileDir, "*.xlsx");
            return TryGenerateWithExcelFileArr(xlsxFileArr, csFileDir);
        }

        public bool TryGenerateWithExcelFileArr(string[] xlsxFileArr, string csFileDir)
        {
            var modelList = new List<Dictionary<string, object>>();
            var parser = new SchemeParser(ProtoExaminerRegistry.TypeExaminerList);

            foreach (var filePath in xlsxFileArr)
            {
                var name = Path.GetFileNameWithoutExtension(filePath);
                if (name.StartsWith("~$") || name.StartsWith("Enum"))
                {
                    continue;
                }

                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var table = ExcelDataParser.ParseExcel(stream, out var subTableDict);

                if (table != null)
                {
                    var model = ParseTable(parser, name, table);
                    modelList.Add(model);
                }
                foreach (var (subTableName, subTable) in subTableDict)
                {
                    var subModel = ParseTable(parser, subTableName, subTable);
                    modelList.Add(subModel);
                }
            }

            Directory.CreateDirectory(csFileDir);
            var templateText = File.ReadAllText(_templatePath);
            var template = Template.Parse(templateText);

            foreach (var model in modelList)
            {
                var contents = template.Render(model);
                var filePath = Path.Combine(csFileDir, model["ClassName"] + ".cs");
                File.WriteAllText(filePath, contents);
            }
            return true;
        }
        public bool TryGenerateWithCsvDir(string csvFileDir, string csFileDir)
        {
            var csvFileArr = Directory.GetFiles(csvFileDir, "*.csv");
            return TryGenerateWithCsvFileArr(csvFileArr, csFileDir);
        }

        public bool TryGenerateWithCsvFileArr(string[] csvFileArr, string csFileDir)
        {
            var modelList = new List<Dictionary<string, object>>();
            var parser = new SchemeParser(ProtoExaminerRegistry.TypeExaminerList);

            foreach (var filePath in csvFileArr)
            {
                var name = Path.GetFileNameWithoutExtension(filePath);
                if (name.StartsWith("~$") || name.StartsWith("Enum"))
                {
                    continue;
                }

                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var table = ExcelDataParser.ParseCsv(stream);

                try
                {
					if (table != null)
					{
						var model = ParseTable(parser, name, table);
						modelList.Add(model);
					}
				}
                catch (Exception e)
                {
                    throw new Exception($"({name}) 프로토 클래스 생성 중 에러가 발생했습니다. Err({e.Message})");
                }
                
            }

            Directory.CreateDirectory(csFileDir);
            var templateText = File.ReadAllText(_templatePath);
            var template = Template.Parse(templateText);

            foreach (var model in modelList)
            {
                var contents = template.Render(model);
                var filePath = Path.Combine(csFileDir, model["ClassName"] + ".cs");
                File.WriteAllText(filePath, contents);
            }

            return true;
        }

        private Dictionary<string, object> ParseTable(ISchemeParser parser, string name, DataTable table)
        {
            var nameList = ProtoGenerator.ReadRow(table, 0)!;
            var schemeList = ProtoGenerator.ReadRow(table, 1)!;

            var schemes = new ProtoScheme[nameList.Count];
            for (var i = 0; i < nameList.Count; ++i)
            {
                schemes[i] = new ProtoScheme { Name = nameList[i], Scheme = schemeList[i] };
            }

            var descriptor = parser.Parse(schemes);
            var propList = descriptor.PropInfoList.Select(propInfo => new Dictionary<string, string>
            {
                { "Name", propInfo.Name },
                { "Type", GetTypeName(propInfo.Type) }
            }).ToList();

            return new Dictionary<string, object>
            {
                { "ClassName", name + "Proto" },
                { "PropertyList",  propList }
            };
        }

        private static string GetTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.FullName!;
            }

            var sb = new StringBuilder();
            sb.Append(type.Name[..type.Name.IndexOf('`')]);
            sb.Append('<');

            var appendComma = false;
            foreach (var arg in type.GetGenericArguments())
            {
                if (appendComma)
                {
                    sb.Append(',');
                }
                sb.Append(GetTypeName(arg));
                appendComma = true;
            }

            sb.Append('>');
            return sb.ToString();
        }

        private string _templatePath = "";
    }
}
