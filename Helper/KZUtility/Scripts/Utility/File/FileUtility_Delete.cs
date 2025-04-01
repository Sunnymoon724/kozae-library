using System;
using System.IO;

namespace KZLib.KZUtility
{
	public static partial class FileUtility
	{
		public static void DeleteEmptyDirectory(string absoluteStartPath,Action? onComplete = null)
		{
			if(!IsFolderExist(absoluteStartPath))
			{
				return;
			}

			_DeleteEmptyDirectory(absoluteStartPath);

			onComplete?.Invoke();
		}

		private static void _DeleteEmptyDirectory(string startPath)
		{
			foreach(var folderPath in GetFolderPathArray(startPath))
			{
				_DeleteEmptyDirectory(folderPath);

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
	}
}