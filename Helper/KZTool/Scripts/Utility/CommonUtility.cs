using ClosedXML.Excel;
using System.IO;
using KZLib.KZUtility;

namespace KZLib.KZTool
{
	internal static class CommonUtility
	{
		internal static void GenerateExcelFile(string folderPath,string fileName,XLWorkbook workbook)
		{
			FileUtility.CreateFolder(folderPath);

			var filePath = Path.Combine(folderPath,$"{fileName}.xlsx");

			workbook.SaveAs(filePath);
		}

		internal static void GenerateTextFile(string filePath,string text)
		{
			FileUtility.WriteTextToFile(filePath,text);
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