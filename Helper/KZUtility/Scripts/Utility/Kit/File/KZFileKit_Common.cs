/// <summary>
/// Static file and folder helpers for Unity project paths, I/O, copy/move/delete, search, and zip compression.
/// </summary>
public static partial class KZFileKit
{
	private const int c_kiloByte = 1 << 10;
	private const int c_megaByte = c_kiloByte * c_kiloByte;

	/// <summary>
	/// Glob patterns for Excel files used by <see cref="IsExcelFile"/> and <see cref="FindExcelFilesInFolder"/>.
	/// </summary>
	public static readonly string[] s_excelSearchPatterns = new string[] { "*.xls", "*.xlsx", "*.xlsm" };
}