using ExcelDataReader;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace NX.Util
{
    public static class ExcelDataParser
    {
        static ExcelDataParser()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static DataTable? ParseCsv(Stream inputStream)
        {
            using var reader = ExcelReaderFactory.CreateCsvReader(inputStream);
            var tables = reader.AsDataSet().Tables;
            return tables.Count > 0 ? tables[0] : null;
        }

        public static DataTable? ParseExcel(Stream inputStream, out Dictionary<string, DataTable> subTableDict)
        {
            using var reader = ExcelReaderFactory.CreateReader(inputStream);
            var dataSet = reader.AsDataSet();

            DataTable? result = null;
            subTableDict = new Dictionary<string, DataTable>();
            foreach (DataTable table in dataSet.Tables)
            {
                if (table.TableName.StartsWith('@'))
                {
                    subTableDict.Add(table.TableName[1..], table);
                }
                else if (table.TableName.StartsWith('+'))
                {
                    if (result != null)
                    {
                        MergeTables(result, table);
                    }
                    else
                    {
                        result = table;
                    }
                }
            }
            return result;
        }

        private static DataTable MergeTables(DataTable dest, DataTable src)
        {
            for (var cntNeedCol = src.Columns.Count - dest.Columns.Count; cntNeedCol >= 0; --cntNeedCol)
            {
                dest.Columns.Add();
            }
            for (var i = 2; i < src.Rows.Count; ++i) // Exclude header
            {
                dest.Rows.Add(src.Rows[i].ItemArray);
            }
            return dest;
        }
    }
}
