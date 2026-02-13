using System;
using System.IO;

namespace KZLib.Utilities
{
	public static partial class FileUtility
	{
		/// <param name="filePath">The absolute file path.</param>
		public static string ReadFileToText(string filePath)
		{
			if(!IsFileExist(filePath))
			{
				return string.Empty;
			}

			return _ReadFile(filePath,File.ReadAllText);
		}

		/// <param name="filePath">The absolute file path.</param>
		public static byte[] ReadFileToBytes(string filePath)
		{
			if(!IsFileExist(filePath))
			{
				return Array.Empty<byte>();
			}

			return _ReadFile(filePath,File.ReadAllBytes);
		}

		private static TRead _ReadFile<TRead>(string filePath,Func<string,TRead> onRead)
		{
			return onRead(filePath);
		}
	}
}