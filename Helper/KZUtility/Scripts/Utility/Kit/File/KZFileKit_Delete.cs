using System;
using System.IO;

public static partial class KZFileKit
{
	/// <summary>
	/// Recursively deletes empty subFolders under <paramref name="absoluteStartPath"/>.
	/// Ignores .DS_Store when deciding whether a folder is empty. Also removes companion .meta files in Unity projects.
	/// </summary>
	public static void DeleteEmptyFolders(string absoluteStartPath,Action? onComplete = null)
	{
		if(!IsFolderExist(absoluteStartPath))
		{
			return;
		}

		_DeleteEmptyFolders(absoluteStartPath);

		onComplete?.Invoke();
	}

	private static void _DeleteEmptyFolders(string startPath)
	{
		foreach(var folderPath in GetFolderPathArray(startPath))
		{
			_DeleteEmptyFolders(folderPath);

			var innerFolderPathArray = GetFolderPathArray(folderPath);

			if(innerFolderPathArray.Length > 0)
			{
				continue;
			}

			var filePathArray = Directory.GetFileSystemEntries(folderPath);

			if(filePathArray.Length != 0 && (filePathArray.Length != 1 || !filePathArray[0].EndsWith(".DS_Store")))
			{
				continue;
			}

			foreach(var filePath in filePathArray)
			{
				DeleteFile(filePath);
			}

			DeleteFile($"{folderPath}.meta");

			Directory.Delete(folderPath,false);
		}
	}

	/// <summary>
	/// Deletes a file and its Unity .meta companion when present.
	/// </summary>
	public static void DeleteFile(string absoluteFilePath,Action? onComplete = null)
	{
		if(!IsFileExist(absoluteFilePath))
		{
			return;
		}

		File.Delete(absoluteFilePath);

		if(absoluteFilePath.Contains(".meta"))
		{
			return;
		}

		var metaFile = $"{absoluteFilePath}.meta";

		DeleteFile(metaFile);

		onComplete?.Invoke();
	}

	/// <summary>
	/// Deletes a folder and its Unity .meta companion when present.
	/// </summary>
	public static void DeleteFolder(string absoluteFolderPath,bool recursive,Action? onComplete = null)
	{
		if(!IsFolderExist(absoluteFolderPath))
		{
			return;
		}

		Directory.Delete(absoluteFolderPath,recursive);

		var metaFile = $"{absoluteFolderPath}.meta";

		DeleteFile(metaFile);

		onComplete?.Invoke();
	}
}
