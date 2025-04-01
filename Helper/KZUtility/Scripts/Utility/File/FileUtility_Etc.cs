using System.IO;
using System.Linq;

namespace KZLib.KZUtility
{
	public static partial class FileUtility
	{
		private const int c_kiloByte = 1 << 10;
		private const int c_megaByte = c_kiloByte * c_kiloByte;

		private static readonly string[] s_excelExtensionArray = new string[] { ".xls", ".xlsx", ".xlsm" };

		public static long GetFileSizeByte(string filePath)
		{
			return !IsFileExist(filePath) ? 0L : new FileInfo(filePath).Length;
		}

		public static long GetFileSizeKB(string filePath)
		{
			return (long) (GetFileSizeByte(filePath)/(double)c_kiloByte);
		}

		public static long GetFileSizeMegaByte(string filePath)
		{
			return (long) (GetFileSizeByte(filePath)/(double)c_megaByte);
		}

		public static bool IsExcelFile(string filePath)
		{
			var fileExtension = Path.GetExtension(filePath).ToLower();

			foreach(var excelExtension in s_excelExtensionArray)
			{
				if(string.Equals(fileExtension,excelExtension))
				{
					return true;
				}
			}

			return false;
		}

		public static string WrapPemFormat(string text,string header)
		{
			return $"-----BEGIN {header}-----\n{text}\n-----END {header}-----";
		}

		public static string UnwrapPemFormat(string text,string header)
		{
			var head = $"-----BEGIN {header}-----\n";
			var tail = $"\n-----END {header}-----";

			var start = text.IndexOf(head);
			var end = text.IndexOf(tail);

			if(start == -1 || end == -1)
			{
				return string.Empty;
			}

			return text[(start+head.Length)..end];
		}
	}
}