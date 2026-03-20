using ClosedXML.Excel;
using System.IO;

namespace KZHelper.ToolKits
{
	internal static class KZCommonKit
	{
		internal static void GenerateExcelFile(string folderPath,string fileName,XLWorkbook workbook)
		{
			KZFileKit.CreateFolder(folderPath);

			var filePath = Path.Combine(folderPath,$"{fileName}.xlsx");

			workbook.SaveAs(filePath);
		}

		internal static void GenerateTextFile(string filePath,string text)
		{
			KZFileKit.WriteTextToFile(filePath,text);
		}

		internal static int ReadInt32(BinaryReader binaryReader)
		{
			return (binaryReader.ReadByte() << 24) | (binaryReader.ReadByte() << 16) | (binaryReader.ReadByte() << 8) | binaryReader.ReadByte();
		}

		internal static short ReadInt16(BinaryReader binaryReader)
		{
			return (short)((binaryReader.ReadByte() << 8) | binaryReader.ReadByte());
		}
	}
}