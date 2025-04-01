using System;
using System.Collections.Generic;

namespace KZLib.KZUtility
{
	public static partial class FileUtility
	{
		/// <summary>
		/// .meta is not included
		/// </summary>
		/// <param name="folderPath">The absolute folder path.</param>
		public static IEnumerable<string> FindFilePathGroup(string folderPath,bool includeSubFolder = false)
		{
			if(!IsFolderExist(folderPath))
			{
				yield break;
			}

			var folderQueue = new Queue<string>();
			folderQueue.Enqueue(folderPath);

			while(folderQueue.Count > 0)
			{
				var currentFolderPath = folderQueue.Dequeue();

				foreach(var filePath in GetFilePathArray(folderPath))
				{
					if(filePath.EndsWith(".meta"))
					{
						continue;
					}

					yield return filePath;
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

		/// <param name="folderPath">The absolute folder path.</param>
		public static string FindFileInFolder(string folderPath,string targetName)
		{
			if(!IsFolderExist(folderPath))
			{
				return string.Empty;
			}

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

				if(result != null)
				{
					return result;
				}
			}

			return string.Empty;
		}

		/// <param name="folderPath">The absolute folder path.</param>
		public static IEnumerable<string> FindExtensionInFolder(string folderPath,string extension)
		{
			if(!IsFolderExist(folderPath))
			{
				yield break;
			}

			foreach(var filePath in GetFilePathArray(folderPath,extension))
			{
				yield return filePath;
			}

			foreach(var subFolderPath in GetFolderPathArray(folderPath))
			{
				foreach(var filePath in FindExtensionInFolder(subFolderPath,extension))
				{
					yield return filePath;
				}
			}
		}

		public static string FindFilePath(List<string> filePathList,string text)
		{
			var filePath = filePathList.Find(x => x.Contains(text));

			return IsPathExist(filePath) ? filePath : string.Empty;
		}
	}
}