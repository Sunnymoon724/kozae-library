using System;
using System.IO;
using System.Text.RegularExpressions;

namespace KZLib.KZUtility
{
	public static partial class FileUtility
	{
		/// <summary>
		/// It is based on the Assets folder.
		/// </summary>
		public static string GetAbsolutePath(string path,bool isIncludeAsset)
		{
			if(!IsPathExist(path))
			{
				return string.Empty;
			}

			//? ex) @"C:~"
			if(Path.IsPathRooted(path))
			{
				return NormalizePath(path);
			}
			else if(isIncludeAsset)
			{
				//? Change AssetPath
				return NormalizePath(Path.GetFullPath(GetAssetPath(path)));
			}
			else
			{
				return NormalizePath(Path.GetFullPath(path,GetProjectParentPath()));
			}
		}

		public static string NormalizePath(string path)
		{
			return path.Replace('/',Path.DirectorySeparatorChar).Replace('\\',Path.DirectorySeparatorChar);
		}

		/// <summary>
		/// File : name+extension / Folder : name
		/// </summary>
		public static string GetFileName(string path)
		{
			return IsPathExist(path) ? Path.GetFileName(path) : string.Empty;
		}

		public static string GetOnlyName(string path)
		{
			return IsPathExist(path) ? Path.GetFileNameWithoutExtension(path) : string.Empty;
		}

		public static string GetExtension(string path)
		{
			return IsPathExist(path) ? Path.GetExtension(path) : string.Empty;
		}

		public static string GetParentPath(string path)
		{
			if(IsPathExist(path))
			{
				var directoryName = Path.GetDirectoryName(path);
				
				if(!string.IsNullOrEmpty(directoryName))
				{
					return directoryName;
				}
			}

			return string.Empty;
		}

		public static string GetProjectPath()
		{
			return Directory.GetCurrentDirectory();
		}

		public static string GetProjectParentPath()
		{
			return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),".."));
		}

		/// <summary>
		/// Remove extension from path
		/// </summary>
		public static string GetPathWithoutExtension(string path)
		{
			return IsPathExist(path) ? Regex.Replace(path,@"\.[^.]*$","") : string.Empty;
		}

		public static string ChangeExtension(string path,string extension)
		{
			return IsPathExist(path) ? Path.ChangeExtension(path,extension) : string.Empty;
		}

		public static string GetParentAbsolutePath(string path,bool isIncludeAsset)
		{
			return IsPathExist(path) ? GetAbsolutePath(GetParentPath(path),isIncludeAsset) : string.Empty;
		}

		/// <summary>
		/// Assets/... -> ... (Local Path)
		/// </summary>
		public static string GetLocalPath(string path)
		{
			if(!IsPathExist(path))
			{
				return string.Empty;
			}

			return IsIncludeAssetHeader(path) ? RemoveAssetHeader(path) : NormalizePath(path);
		}

		public static string GetAssetPath(string path)
		{
			if(!IsPathExist(path))
			{
				return string.Empty;
			}

			return Path.Combine("Assets",GetLocalPath(path));
		}

		public static bool IsIncludeAssetHeader(string path)
		{
			if(!IsPathExist(path))
			{
				return false;
			}

			return path.Contains("Assets");
		}

		public static bool IsStartWithAssetHeader(string path)
		{
			if(!IsPathExist(path))
			{
				return false;
			}

			return path.StartsWith("Assets");
		}

		public static bool IsFilePath(string filePath)
		{
			if(!IsPathExist(filePath))
			{
				return false;
			}

			return Path.HasExtension(filePath);
		}

		public static string[] GetFilePathArray(string folderPath,string pattern = "")
		{
			if(!IsPathExist(folderPath))
			{
				return Array.Empty<string>();
			}

			return string.IsNullOrEmpty(pattern) ? Directory.GetFiles(folderPath) : Directory.GetFiles(folderPath,pattern);
		}

		public static string[] GetFolderPathArray(string folderPath,string pattern = "")
		{
			if(!IsFolderExist(folderPath))
			{
				return Array.Empty<string>();
			}

			return string.IsNullOrEmpty(pattern) ? Directory.GetDirectories(folderPath) : Directory.GetDirectories(folderPath,pattern);
		}

		public static bool IsPathExist(string path)
		{
			return !string.IsNullOrEmpty(path);
		}

		public static bool IsFileExist(string absoluteFilePath)
		{
			if(!IsPathExist(absoluteFilePath))
			{
				return false;
			}

			return File.Exists(absoluteFilePath);
		}

		public static bool IsFolderExist(string absoluteFolderPath)
		{
			if(!IsPathExist(absoluteFolderPath))
			{
				return false;
			}

			return Directory.Exists(absoluteFolderPath);
		}

		public static string RemoveHeaderInPath(string path,string header)
		{
			if(!IsPathExist(path))
			{
				return path;
			}

			if(string.IsNullOrEmpty(header))
			{
				return path;
			}

			var normalizedPath = NormalizePath(path);
			var normalizedHeader = NormalizePath(header);

			if(!normalizedPath.StartsWith(normalizedHeader))
			{
				return path;
			}

			var headerLength = normalizedHeader.Length;

			if(path.Length > headerLength && (normalizedPath[headerLength] == Path.DirectorySeparatorChar))
			{
				headerLength++;
			}

			return path[headerLength..];
		}

		public static string RemoveAssetHeader(string path)
		{
			if(!IsPathExist(path))
			{
				return string.Empty;
			}

			return RemoveHeaderInPath(path,"Assets");
		}

		private static string _GetUniquePath(string path)
		{
			if(!IsPathExist(path))
			{
				return string.Empty;
			}

			var directory = GetParentPath(path);
			var name = GetOnlyName(path);
			var extension = GetExtension(path);

			var count = 1;
			var newPath = path;

			while(IsFileExist(newPath))
			{
				newPath = Path.Combine(directory,$"{name} ({count}){extension}");
				count++;
			}

			return newPath;
		}
	}
}