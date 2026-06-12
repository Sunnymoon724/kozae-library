using System.IO;
using System.Text.RegularExpressions;

public static partial class KZFileKit
{
	/// <summary>
	/// Resolves <paramref name="path"/> to an absolute path.
	/// Rooted paths are normalized directly; relative paths use the Assets folder or project parent when <paramref name="isIncludeAsset"/> is false.
	/// Returns an empty string when <paramref name="path"/> is empty.
	/// </summary>
	public static string GetAbsolutePath(string path,bool isIncludeAsset)
	{
		if(!IsValidPathString(path))
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

	/// <summary>
	/// Normalizes directory separators to the platform separator.
	/// </summary>
	public static string NormalizePath(string path)
	{
		return path.Replace('/',Path.DirectorySeparatorChar).Replace('\\',Path.DirectorySeparatorChar);
	}

	/// <summary>
	/// Returns the file or folder name: file name with extension for files, folder name for directories.
	/// </summary>
	public static string GetFileName(string path)
	{
		return IsValidPathString(path) ? Path.GetFileName(path) : string.Empty;
	}

	/// <summary>
	/// Returns the file name without extension, or an empty string when <paramref name="path"/> is empty.
	/// </summary>
	public static string GetOnlyFileName(string path)
	{
		return IsValidPathString(path) ? Path.GetFileNameWithoutExtension(path) : string.Empty;
	}

	/// <summary>
	/// Returns the file extension including the leading dot, or an empty string when <paramref name="path"/> is empty.
	/// </summary>
	public static string GetExtension(string path)
	{
		return IsValidPathString(path) ? Path.GetExtension(path) : string.Empty;
	}

	/// <summary>
	/// Returns the parent directory path, or an empty string when unavailable.
	/// </summary>
	public static string GetParentPath(string path)
	{
		if(IsValidPathString(path))
		{
			var directoryName = Path.GetDirectoryName(path);
			
			if(!string.IsNullOrEmpty(directoryName))
			{
				return directoryName;
			}
		}

		return string.Empty;
	}

	/// <summary>
	/// Returns the current working directory (Unity project root in the editor).
	/// </summary>
	public static string GetProjectPath()
	{
		return Directory.GetCurrentDirectory();
	}

	/// <summary>
	/// Returns the parent directory of the current working directory.
	/// </summary>
	public static string GetProjectParentPath()
	{
		return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),".."));
	}

	/// <summary>
	/// Removes the last extension segment from <paramref name="path"/>.
	/// </summary>
	public static string GetPathWithoutExtension(string path)
	{
		return IsValidPathString(path) ? Regex.Replace(path,@"\.[^.]*$","") : string.Empty;
	}

	/// <summary>
	/// Changes the extension of <paramref name="path"/> to <paramref name="extension"/>.
	/// </summary>
	public static string ChangeExtension(string path,string extension)
	{
		return IsValidPathString(path) ? Path.ChangeExtension(path,extension) : string.Empty;
	}

	/// <summary>
	/// Returns the absolute parent path of <paramref name="path"/> using the same rules as <see cref="GetAbsolutePath"/>.
	/// </summary>
	public static string GetParentAbsolutePath(string path,bool isIncludeAsset)
	{
		return IsValidPathString(path) ? GetAbsolutePath(GetParentPath(path),isIncludeAsset) : string.Empty;
	}

	/// <summary>
	/// Converts an Assets-relative path to a local path without the Assets prefix.
	/// </summary>
	public static string GetLocalPath(string path)
	{
		if(!IsValidPathString(path))
		{
			return string.Empty;
		}

		return IsIncludeAssetHeader(path) ? RemoveAssetHeader(path) : NormalizePath(path);
	}

	/// <summary>
	/// Combines <paramref name="path"/> under the Assets folder.
	/// </summary>
	public static string GetAssetPath(string path)
	{
		if(!IsValidPathString(path))
		{
			return string.Empty;
		}

		return Path.Combine("Assets",GetLocalPath(path));
	}

	/// <summary>
	/// Returns whether <paramref name="path"/> contains an Assets segment anywhere.
	/// </summary>
	public static bool IsIncludeAssetHeader(string path)
	{
		if(!IsValidPathString(path))
		{
			return false;
		}

		return path.Contains("Assets");
	}

	/// <summary>
	/// Returns whether <paramref name="path"/> starts with Assets.
	/// </summary>
	public static bool IsStartWithAssetHeader(string path)
	{
		if(!IsValidPathString(path))
		{
			return false;
		}

		return path.StartsWith("Assets");
	}

	/// <summary>
	/// Returns whether <paramref name="filePath"/> has a file extension in the path string.
	/// </summary>
	public static bool HasFileExtension(string filePath)
	{
		if(!IsValidPathString(filePath))
		{
			return false;
		}

		return Path.HasExtension(filePath);
	}

	/// <summary>
	/// Removes <paramref name="header"/> and a following separator from the start of <paramref name="path"/> when present.
	/// </summary>
	public static string RemoveHeaderInPath(string path,string header)
	{
		if(!IsValidPathString(path))
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

	/// <summary>
	/// Removes the Assets prefix from <paramref name="path"/>.
	/// </summary>
	public static string RemoveAssetHeader(string path)
	{
		if(!IsValidPathString(path))
		{
			return string.Empty;
		}

		return RemoveHeaderInPath(path,"Assets");
	}
}
