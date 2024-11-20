using System;
using System.IO;

namespace KZLib.Utility
{
	public static partial class FileUtility
	{
		/// <param name="_startPath">The absolute path of the folder.</param>
		public static void DeleteEmptyDirectory(string _startPath,Action _onComplete = null)
		{
			IsFolderExist(_startPath);

			DeleteEmptyDirectoryInner(_startPath);

			_onComplete?.Invoke();
		}

		private static void DeleteEmptyDirectoryInner(string _startPath)
		{
			foreach(var folderPath in GetFolderPathArray(_startPath))
			{
				DeleteEmptyDirectoryInner(folderPath);

				var innerFolderPathArray = GetFolderPathArray(folderPath);

				if(innerFolderPathArray.Length > 0)
				{
					continue;
				}

				var filePathArray = Directory.GetFileSystemEntries(folderPath);

				if(filePathArray.Length == 0 || (filePathArray.Length == 1 && filePathArray[0].EndsWith(".DS_Store")))
				{
					foreach(var filePath in filePathArray)
					{
						DeleteFile(filePath);
					}

					DeleteFile($"{folderPath}.meta");

					Directory.Delete(folderPath,false);
				}
			}
		}

		/// <param name="_filePath">The absolute path of the file.</param>
		public static void DeleteFile(string _filePath,Action _onComplete = null)
		{
			IsFileExist(_filePath);

			File.Delete(_filePath);

			if(_filePath.Contains(".meta"))
			{
				return;
			}

			var meta = $"{_filePath}.meta";

			DeleteFile(meta);

			_onComplete?.Invoke();
		}
	}
}