using System;
using System.Collections.Generic;
using System.IO;

namespace KZLib.KZUtility
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

			return ReadFile(filePath,File.ReadAllText);
		}

		/// <param name="filePath">The absolute file path.</param>
		public static byte[] ReadFileToBytes(string filePath)
		{
			if(!IsFileExist(filePath))
			{
				return Array.Empty<byte>();
			}

			return ReadFile(filePath,File.ReadAllBytes);
		}

		private static TRead ReadFile<TRead>(string filePath,Func<string,TRead> onRead)
		{
			return onRead(filePath);
		}

		/// <param name="filePath">The absolute file path.</param>
		public static IEnumerable<string> ReadExcelFileGroupByFolderPath(string folderPath)
		{
			foreach(var filePath in ReadFileGroupByFolderPath(folderPath,s_excelExtensionArray))
			{
				yield return filePath;
			}
		}

		/// <param name="filePath">The absolute file path.</param>
		public static IEnumerable<string> ReadFileGroupByFolderPath(string folderPath,string[] extensionArray)
		{
			if(!IsFolderExist(folderPath))
			{
				yield break;
			}

			foreach(var extension in extensionArray)
			{
				foreach(var filePath in Directory.GetFiles(folderPath,extension))
				{
					yield return filePath;
				}
			}
		}
	}
}