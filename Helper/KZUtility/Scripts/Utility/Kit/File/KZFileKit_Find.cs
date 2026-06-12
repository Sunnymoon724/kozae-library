using System.Collections.Generic;
using System.IO;

public static partial class KZFileKit
{
	/// <summary>
	/// Yields file paths under <paramref name="folderPath"/>, excluding .meta files.
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

	/// <summary>
	/// Recursively finds the first file whose name equals <paramref name="targetName"/>.
	/// </summary>
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

	/// <summary>
	/// Returns whether <paramref name="absoluteFilePath"/> exists and has an Excel extension (.xls, .xlsx, .xlsm).
	/// </summary>
	public static bool IsExcelFile(string absoluteFilePath)
	{
		if(IsFileExist(absoluteFilePath))
		{
			var fileExtension = Path.GetExtension(absoluteFilePath).ToLower();

			foreach(var excelPattern in s_excelSearchPatterns)
			{
				if(string.Equals($"*{fileExtension}",excelPattern))
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Recursively yields Excel files under <paramref name="folderPath"/>, skipping Excel lock files (~$).
	/// </summary>
	public static IEnumerable<string> FindExcelFilesInFolder(string folderPath)
	{
		foreach(var filePath in FindFileGroup(folderPath,s_excelSearchPatterns,true))
		{
			var fileName = GetFileName(filePath);

			if(fileName.StartsWith("~$"))
			{
				continue;
			}

			yield return filePath;
		}
	}

	/// <summary>
	/// Yields files in <paramref name="folderPath"/> matching <paramref name="searchPatterns"/>.
	/// </summary>
	public static IEnumerable<string> FindFileGroup(string folderPath,string[] searchPatterns,bool recursive = false)
	{
		if(!IsFolderExist(folderPath))
		{
			yield break;
		}

		if(recursive)
		{
			var folderQueue = new Queue<string>();

			folderQueue.Enqueue(folderPath);

			while(folderQueue.Count > 0)
			{
				var currentFolderPath = folderQueue.Dequeue();

				foreach(var pattern in searchPatterns)
				{
					foreach(var filePath in GetFilePathArray(currentFolderPath,pattern))
					{
						if(!string.IsNullOrEmpty(filePath))
						{
							yield return filePath;
						}
					}
				}

				foreach(var subFolderPath in GetFolderPathArray(currentFolderPath))
				{
					if(!string.IsNullOrEmpty(subFolderPath))
					{
						folderQueue.Enqueue(subFolderPath);
					}
				}
			}
		}
		else
		{
			foreach(var pattern in searchPatterns)
			{
				foreach(var filePath in GetFilePathArray(folderPath,pattern))
				{
					if(!string.IsNullOrEmpty(filePath))
					{
						yield return filePath;
					}
				}
			}
		}
	}

	/// <summary>
	/// Recursively yields files matching <paramref name="searchPattern"/> under <paramref name="folderPath"/>.
	/// </summary>
	public static IEnumerable<string> FindFileGroup(string folderPath,string searchPattern)
	{
		foreach(var filePath in FindFileGroup(folderPath,new string[] { searchPattern },true))
		{
			yield return filePath;
		}
	}

	/// <summary>
	/// Finds the first path in <paramref name="filePathList"/> whose file name without extension equals <paramref name="name"/>.
	/// </summary>
	public static string FindPathByFileName(List<string> filePathList,string name)
	{
		for(var i=0;i<filePathList.Count;i++)
		{
			var filePath = filePathList[i];

			if(!HasFileExtension(filePath))
			{
				continue;
			}

			var fileName = GetOnlyFileName(filePath);

			if(string.Equals(fileName,name))
			{
				return filePath;
			}
		}

		return string.Empty;
	}
}
