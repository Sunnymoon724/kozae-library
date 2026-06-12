using System;
using System.IO;

public static partial class KZFileKit
{
	/// <summary>
	/// Returns file paths in <paramref name="folderPath"/>, optionally filtered by <paramref name="pattern"/>.
	/// </summary>
	public static string[] GetFilePathArray(string folderPath,string pattern = "")
	{
		if(!IsFolderExist(folderPath))
		{
			return Array.Empty<string>();
		}

		return string.IsNullOrEmpty(pattern) ? Directory.GetFiles(folderPath) : Directory.GetFiles(folderPath,pattern);
	}

	/// <summary>
	/// Returns subfolder paths in <paramref name="folderPath"/>, optionally filtered by <paramref name="pattern"/>.
	/// </summary>
	public static string[] GetFolderPathArray(string folderPath,string pattern = "")
	{
		if(!IsFolderExist(folderPath))
		{
			return Array.Empty<string>();
		}

		return string.IsNullOrEmpty(pattern) ? Directory.GetDirectories(folderPath) : Directory.GetDirectories(folderPath,pattern);
	}

	/// <summary>
	/// Returns a path that does not collide with an existing file or folder by appending (n) before the extension.
	/// </summary>
	private static string _GetUniquePath(string path)
	{
		if(!IsValidPathString(path))
		{
			return string.Empty;
		}

		var directory = GetParentPath(path);
		var name = GetOnlyFileName(path);
		var extension = GetExtension(path);

		var count = 1;
		var newPath = path;

		while(IsFileExist(newPath) || IsFolderExist(newPath))
		{
			newPath = Path.Combine(directory,$"{name} ({count}){extension}");
			count++;
		}

		return newPath;
	}
}
