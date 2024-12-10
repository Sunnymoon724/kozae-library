using System;
using System.Collections.Generic;
using System.IO;

namespace KZLib.KZTool
{
	internal static partial class Utility
	{
		private const string EXTENSION = ".generated.cs";

		/// <summary>
		/// File : name+extension / Folder : name
		/// </summary>
		internal static string GetFileName(string filePath)
		{
			IsPathExist(filePath);

			return Path.GetFileName(filePath);
		}

		internal static IEnumerable<string> GetExcelFilePathGroup(string folderPath)
		{
			IsFolderExist(folderPath);

			foreach(var extension in new string[] { "*.xls", "*.xlsx", "*.xlsm" })
			{
				foreach(var filePath in Directory.GetFiles(folderPath,extension))
				{
					yield return filePath;
				}
			}
		}

		private static bool IsPathExist(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				throw new NullReferenceException("Path is null");
			}

			return true;
		}

		internal static bool IsFileExist(string filePath)
		{
			IsPathExist(filePath);

			var result = File.Exists(filePath);

			if(!result)
			{
				throw new NullReferenceException($"{filePath} is not exist");
			}

			return result;
		}

		internal static bool IsFolderExist(string folderPath)
		{
			IsPathExist(folderPath);

			var result = Directory.Exists(folderPath);

			if(!result)
			{
				throw new NullReferenceException($"{folderPath} is not exist");
			}

			return result;
		}

		private static bool IsFilePath(string filePath)
		{
			IsPathExist(filePath);

			return Path.HasExtension(filePath);
		}

		private static string GetParentPath(string path)
		{
			IsPathExist(path);

			return Path.GetDirectoryName(path);
		}

		/// <summary>
		/// Create folder. (path is file ? create parent folder. : create folder)
		/// </summary>
		internal static void CreateFolder(string path)
		{
			IsPathExist(path);

			// Path is file ? Get parent path. : Get path
			var folderPath = IsFilePath(path) ? GetParentPath(path) : path;

			Directory.CreateDirectory(folderPath);
		}

		internal static void WriteTextToFile(string folderPath,string fileName,string text)
		{
			var filePath = Path.Combine(folderPath,$"{fileName}{EXTENSION}");

			CreateFolder(filePath);

			File.WriteAllText(filePath,text);
		}
	}
}