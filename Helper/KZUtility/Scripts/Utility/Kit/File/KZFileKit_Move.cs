using System.IO;

public static partial class KZFileKit
{
	/// <summary>
	/// Moves a file into <paramref name="destinationFolderPath"/> using the original file name.
	/// When the destination file exists, replaces it only if <paramref name="isOverride"/> is true.
	/// </summary>
	/// <param name="sourceFilePath">The absolute path of the source file.</param>
	/// <param name="destinationFolderPath">The absolute path of the destination folder.</param>
	public static void MoveFile(string sourceFilePath,string destinationFolderPath,bool isOverride)
	{
		//? not exist file -> return
		if(!IsFileExist(sourceFilePath))
		{
			return;
		}

		var fileName = GetFileName(sourceFilePath);
		var destinationFilePath = Path.Combine(destinationFolderPath,fileName);

		CreateFolder(destinationFolderPath);

		//? exist file -> return
		if(IsFileExist(destinationFilePath))
		{
			if(!isOverride)
			{
				return;
			}

			DeleteFile(destinationFilePath);
		}

		File.Move(sourceFilePath,destinationFilePath);
	}

	/// <summary>
	/// Moves immediate child files of <paramref name="sourceFolderPath"/> into <paramref name="destinationFolderPath"/>.
	/// Subfolders are not moved.
	/// </summary>
	/// <param name="sourceFolderPath">The absolute path of the source folder.</param>
	/// <param name="destinationFolderPath">The absolute path of the destination folder.</param>
	public static void MoveFilesInFolder(string sourceFolderPath,string destinationFolderPath,bool isOverride)
	{
		if(!IsFolderExist(sourceFolderPath))
		{
			return;
		}

		CreateFolder(destinationFolderPath);

		foreach(var filePath in GetFilePathArray(sourceFolderPath,"*.*"))
		{
			MoveFile(filePath,destinationFolderPath,isOverride);
		}
	}
}
