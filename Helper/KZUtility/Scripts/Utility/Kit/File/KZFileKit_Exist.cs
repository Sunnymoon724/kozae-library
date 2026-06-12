using System.IO;

public static partial class KZFileKit
{
	/// <summary>
	/// Returns whether the path string is non-null and non-empty. Does not check disk existence.
	/// </summary>
	public static bool IsValidPathString(string path)
	{
		return !string.IsNullOrEmpty(path);
	}

	/// <summary>
	/// Returns whether a file exists at <paramref name="absoluteFilePath"/>.
	/// </summary>
	public static bool IsFileExist(string absoluteFilePath)
	{
		if(!IsValidPathString(absoluteFilePath))
		{
			return false;
		}

		return File.Exists(absoluteFilePath);
	}

	/// <summary>
	/// Returns whether a directory exists at <paramref name="absoluteFolderPath"/>.
	/// </summary>
	public static bool IsFolderExist(string absoluteFolderPath)
	{
		if(!IsValidPathString(absoluteFolderPath))
		{
			return false;
		}

		return Directory.Exists(absoluteFolderPath);
	}
}
