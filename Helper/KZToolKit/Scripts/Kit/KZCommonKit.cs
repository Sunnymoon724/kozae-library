using ClosedXML.Excel;
using System.IO;

namespace KZHelper.ToolKits
{
	internal static class KZCommonKit
	{
		private const int c_byte3Shift = 24;
		private const int c_byte2Shift = 16;
		private const int c_byte1Shift = 8;

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
			return (binaryReader.ReadByte() << c_byte3Shift) | (binaryReader.ReadByte() << c_byte2Shift) | (binaryReader.ReadByte() << c_byte1Shift) | binaryReader.ReadByte();
		}

		internal static short ReadInt16(BinaryReader binaryReader)
		{
			return (short)((binaryReader.ReadByte() << c_byte1Shift) | binaryReader.ReadByte());
		}
	}
}