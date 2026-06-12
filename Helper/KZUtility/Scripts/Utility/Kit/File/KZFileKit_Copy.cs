using System.IO;

public static partial class KZFileKit
{
	/// <summary>
	/// Copies a file into <paramref name="destinationFolderPath"/> using the original file name.
	/// Skips when the destination file exists and <paramref name="isOverride"/> is false.
	/// </summary>
	/// <param name="sourceFilePath">The absolute path of the source file.</param>
	/// <param name="destinationFolderPath">The absolute path of the destination folder.</param>
	public static void CopyFile(string sourceFilePath,string destinationFolderPath,bool isOverride)
	{
		if(!IsFileExist(sourceFilePath))
		{
			return;
		}

		var fileName = GetFileName(sourceFilePath);
		var destinationFilePath = Path.Combine(destinationFolderPath,fileName);

		if(IsFileExist(destinationFilePath) && !isOverride)
		{
			return;
		}

		File.Copy(sourceFilePath,destinationFilePath,isOverride);
	}

	/// <summary>
	/// Copies immediate child files of <paramref name="sourceFolderPath"/> into <paramref name="destinationFolderPath"/>.
	/// SubFolders are not copied.
	/// </summary>
	/// <param name="sourceFolderPath">The absolute path of the source folder.</param>
	/// <param name="destinationFolderPath">The absolute path of the destination folder.</param>
	public static void CopyFilesInFolder(string sourceFolderPath,string destinationFolderPath,bool isOverride)
	{
		if(!IsFolderExist(sourceFolderPath))
		{
			return;
		}

		CreateFolder(destinationFolderPath);

		foreach(var filePath in GetFilePathArray(sourceFolderPath,"*.*"))
		{
			CopyFile(filePath,destinationFolderPath,isOverride);
		}
	}
}
