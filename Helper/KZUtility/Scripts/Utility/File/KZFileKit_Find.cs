using System.Collections.Generic;
using System.IO;

public static partial class KZFileKit
{
	/// <summary>
	/// .meta is not included
	/// </summary>
	/// <param name="folderPath">The absolute folder path.</param>
	public static IEnumerable<string> FindFilePathGroup(string folderPath,bool includeSubFolder = false)
	{
		if(IsFolderExist(folderPath))
		{
			var folderQueue = new Queue<string>();

			folderQueue.Enqueue(folderPath);

			while(folderQueue.Count > 0)
			{
				var currentFolderPath = folderQueue.Dequeue();

				foreach(var filePath in GetFilePathArray(currentFolderPath))
				{
					if(!string.IsNullOrEmpty(filePath) && !filePath.EndsWith(".meta"))
					{
						yield return filePath;
					}
				}

				if(includeSubFolder)
				{
					foreach(var subFolderPath in GetFolderPathArray(currentFolderPath))
					{
						folderQueue.Enqueue(subFolderPath);
					}
				}
			}
		}
	}

	/// <param name="folderPath">The absolute folder path.</param>
	public static string FindFileInFolder(string folderPath,string targetName)
	{
		if(IsFolderExist(folderPath))
		{
			foreach(var filePath in GetFilePathArray(folderPath))
			{
				var fileName = GetFileName(filePath);

				if(string.Equals(fileName,targetName))
				{
					return filePath;
				}
			}

			foreach(var subFolderPath in GetFolderPathArray(folderPath))
			{
				var result = FindFileInFolder(subFolderPath,targetName);

				if(!string.IsNullOrEmpty(result))
				{
					return result;
				}
			}
		}

		return string.Empty;
	}

	public static IEnumerable<string> FindAllExcelFileGroupByFolderPath(string absoluteFolderPath)
	{
		foreach(var filePath in FindAllFileGroupByFolderPath(absoluteFolderPath,s_excelExtensionArray))
		{
			var fileName = GetFileName(filePath);

			if(fileName.StartsWith("~$"))
			{
				continue;
			}

			yield return filePath;
		}
	}

	public static IEnumerable<string> FindAllFileGroupByFolderPath(string absoluteFolderPath,string[] extensionArray)
	{
		if(IsFolderExist(absoluteFolderPath))
		{
			foreach(var extension in extensionArray)
			{
				foreach(var filePath in GetFilePathArray(absoluteFolderPath,extension))
				{
					if(!string.IsNullOrEmpty(filePath))
					{
						yield return filePath;
					}
				}
			}
		}
	}

	public static IEnumerable<string> FindAllExtensionGroupInFolder(string absoluteFolderPath,string extension)
	{
		var folderQueue = new Queue<string>();

		folderQueue.Enqueue(absoluteFolderPath);

		while(folderQueue.Count > 0)
		{
			var folderPath = folderQueue.Dequeue();

			foreach (var filePath in GetFilePathArray(folderPath, extension))
			{
				if(!string.IsNullOrEmpty(filePath))
				{
					yield return filePath;
				}
			}

			foreach (var subFolderPath in GetFolderPathArray(folderPath))
			{
				if(!string.IsNullOrEmpty(subFolderPath))
				{
					folderQueue.Enqueue(subFolderPath);
				}
			}
		}
	}

	public static string FindFilePath(List<string> filePathList,string name)
	{
		for(var i=0;i<filePathList.Count;i++)
		{
			var filePath = filePathList[i];

			if(!IsFilePath(filePath))
			{
				continue;
			}

			var fileName = GetOnlyName(filePath);

			if(string.Equals(fileName,name))
			{
				return filePath;
			}
		}

		return string.Empty;
	}
}