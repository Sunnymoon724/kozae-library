using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace KZLib.Utility
{
	public static partial class FileUtility
	{
		/// <summary>
		/// Combine all path
		/// </summary>
		public static string PathCombine(params string[] _pathArray)
		{
			return NormalizePath(Path.Combine(_pathArray));
		}

		public static string NormalizePath(string _path)
		{
			return _path.Replace("\\","/");
		}

		/// <summary>
		/// File : name+extension / Folder : name
		/// </summary>
		public static string GetFileName(string _path)
		{
			IsPathExist(_path);

			return Path.GetFileName(_path);
		}

		public static string GetOnlyName(string _path)
		{
			IsPathExist(_path);

			return Path.GetFileNameWithoutExtension(_path);
		}

		public static string GetExtension(string _path)
		{
			IsPathExist(_path);

			return Path.GetExtension(_path);
		}

		public static string GetParentPath(string _path)
		{
			IsPathExist(_path);

			return Path.GetDirectoryName(_path);
		}

		/// <summary>
		/// Remove extension from path
		/// </summary>
		public static string GetPathWithoutExtension(string _path)
		{
			IsPathExist(_path);

			return Regex.Replace(_path,@"\.[^.]*$","");
		}

		public static string ChangeExtension(string _path,string _extension)
		{
			IsPathExist(_path);

			return Path.ChangeExtension(_path,_extension);
		}

		public static bool IsFilePath(string _filePath)
		{
			IsPathExist(_filePath);

			return Path.HasExtension(_filePath);
		}

		public static string[] GetFilePathArray(string _folderPath,string _pattern = null)
		{
			IsPathExist(_folderPath);

			return _pattern == null ? Directory.GetFiles(_folderPath) : Directory.GetFiles(_folderPath,_pattern);
		}

		public static string[] GetFolderPathArray(string _folderPath,string _pattern = null)
		{
			IsPathExist(_folderPath);

			return _pattern == null ? Directory.GetDirectories(_folderPath) : Directory.GetDirectories(_folderPath,_pattern);
		}

		public static bool IsPathExist(string _path)
		{
			if(string.IsNullOrEmpty(_path))
			{
				throw new NullReferenceException("Path is null");
			}

			return true;
		}

		public static bool IsFileExist(string _filePath)
		{
			IsPathExist(_filePath);

			var result = File.Exists(_filePath);

			if(!result)
			{
				throw new NullReferenceException($"{_filePath} is not file path");
			}

			return result;
		}

		public static bool IsFolderExist(string _folderPath)
		{
			IsPathExist(_folderPath);

			var result = Directory.Exists(_folderPath);

			if(!result)
			{
				throw new NullReferenceException($"{_folderPath} is not file path");
			}

			return result;
		}

		public static string[] GetFiles(string _folderPath,string _pattern = null)
		{
			IsFolderExist(_folderPath);

			return Directory.GetFiles(_folderPath,_pattern ?? "");
		}

		public static IEnumerable<string> GetExcelFileGroup(string _folderPath)
		{
			foreach(var filePath in GetFilePathArray(_folderPath,"*.xls;*.xlsx;*.xlsm"))
			{
				var name = Path.GetFileNameWithoutExtension(filePath);

				if(name.StartsWith("~$"))
				{
					continue;
				}

				yield return filePath;
			}
		}

		public static string RemoveHeaderDirectory(string _path,string _header)
		{
			IsPathExist(_path);

			var path = NormalizePath(_path);
			var header = NormalizePath(_header);

			return path[(path.IndexOf(header)+header.Length+1)..];
		}

		private static string GetUniquePath(string _path)
		{
			IsPathExist(_path);

			var directory = GetParentPath(_path);
			var name = GetOnlyName(_path);
			var extension = GetExtension(_path);

			var count = 1;
			var newPath = _path;

			while(IsFileExist(newPath))
			{
				newPath = PathCombine(directory,$"{name} ({count}){extension}");
				count++;
			}

			return newPath;
		}
	}
}