
namespace KZConsole
{
	public class SheetConvertException(string message,string filePath,string sheetName,int lineOrder) : InvalidCastException($"{message} [file{filePath}/sheet[{sheetName}/line:{lineOrder}]") { }
}