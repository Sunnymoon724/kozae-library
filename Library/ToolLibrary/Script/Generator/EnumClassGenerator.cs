using NX.Data;
using Scriban;
using NX.Util.Helpers;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GSAGenerator
{
    public class EnumClassGenerator
    {
        private class EnumProto
        {
            public string Namespace { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public int Value { get; set; } = 0;
            public L10nField Name { get; set; } = new L10nField();
            public string Comment { get; set; } = string.Empty;
        }

        public string Tag { get; set; }

        public EnumClassGenerator(string tag = "")
        {
            Tag = tag;
        }

        public bool LoadTemplate(string templatePath)
        {
            _templatePath = templatePath;
            return true;
        }

        public bool TryGenerateWithExcelFile(string xlsxFilePath, string csFileDir)
        {
            var proto = ExcelProtoListHelper.Deserialize<EnumProto>(xlsxFilePath, true, false, Tag);
            return TryGenerate(proto, csFileDir);
        }
        public bool TryGenerateWithCSVFile(string csvFilePath, string csFileDir)
        {
            using var stream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var proto = CsvProtoListHelper.Deserialize<EnumProto>(stream, true, false, Tag);
            return TryGenerate(proto, csFileDir);
        }

        private bool TryGenerate(NX.ProtoCollection<EnumProto> proto, string csFileDir)
        {
            var enumNamespaceDict = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (var row in proto.Data)
            {
                if (!enumNamespaceDict.TryGetValue(row.Namespace, out var enumDefList))
                {
                    enumDefList = new List<Dictionary<string, string>>();
                    enumNamespaceDict.Add(row.Namespace, enumDefList);
                }

                enumDefList.Add(new Dictionary<string, string>
                {
                    { "Key", row.Key },
                    { "Value", row.Value.ToString() },
                    { "Comment", GetComment(row) },
                });
            }

            var enumNamespaceList = enumNamespaceDict.Select(enumNamespace => new Dictionary<string, object>
            {
                { "Namespace", enumNamespace.Key },
                { "EnumProtoList", enumNamespace.Value }
            }).ToList();

            var modelDict = new Dictionary<string, object>
            {
                { "EnumNamespaceList", enumNamespaceList }
            };

            Directory.CreateDirectory(csFileDir);
            var templateText = File.ReadAllText(_templatePath);
            var template = Template.Parse(templateText);

            var contents = template.Render(modelDict);
            var filePath = Path.Combine(csFileDir, "EnumProto.cs");
            File.WriteAllText(filePath, contents);
            return true;
        }

        private static string GetComment(EnumProto scheme) => scheme.Name.IsValueEmpty() ? scheme.Comment : scheme.Name.ValueText;

        private string _templatePath = "";
    }
}
