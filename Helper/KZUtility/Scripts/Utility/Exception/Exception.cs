
using System;

namespace KZLib.Utilities
{
	public class KZSheetException : InvalidCastException
	{
		public KZSheetException(string message,string filePath,string sheetName,int lineOrder) : base($"{message} [file{filePath}/sheet[{sheetName}/line:{lineOrder}]") { }
	}
}