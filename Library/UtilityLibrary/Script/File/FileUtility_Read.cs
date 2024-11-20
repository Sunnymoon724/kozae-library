using System;
using System.IO;

namespace KZLib.Utility
{
	public static partial class FileUtility
	{
		public static string ReadFileToText(string _filePath)
		{
			IsFileExist(_filePath);

			return ReadFile(_filePath,File.ReadAllText);
		}

		/// <param name="_filePath">The absolute file path.</param>
		public static byte[] ReadFileToBytes(string _filePath)
		{
			IsFileExist(_filePath);

			return ReadFile(_filePath,File.ReadAllBytes);
		}

		private static TRead ReadFile<TRead>(string _filePath,Func<string,TRead> _onRead)
		{
			return _onRead(_filePath);
		}
	}
}