using System.IO;

public static partial class KZFileKit
{
	/// <param name="sourcePath">The absolute path of the file.</param>
	/// <param name="_destinationPath">The absolute path of the folder.</param>
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

	/// <param name="sourcePath">The absolute path of the folder.</param>
	/// <param name="_destinationPath">The absolute path of the folder.</param>
	public static void MoveFolder(string sourceFolderPath,string destinationFolderPath,bool isOverride)
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