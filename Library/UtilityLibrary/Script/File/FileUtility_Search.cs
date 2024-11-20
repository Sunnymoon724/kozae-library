using System.Collections.Generic;

namespace KZLib.Utility
{
	public static partial class FileUtility
	{
		/// <summary>
		/// .meta is not included
		/// </summary>
		public static IEnumerable<string> GetAllFilePathInFolder(string _folderPath,bool _includeSubFolders = false)
		{
			IsFolderExist(_folderPath);

			if(_includeSubFolders)
			{
				foreach(var folderPath in GetFolderPathArray(_folderPath))
				{
					foreach(var filePath in GetAllFilePathInFolder(folderPath,true))
					{
						yield return filePath;
					}
				}
			}

			foreach(var filePath in GetFilePathArray(_folderPath))
			{
				if(filePath.EndsWith(".meta"))
				{
					continue;
				}

				yield return filePath;
			}
		}

		public static string SearchFileInFolder(string _folderPath,string _fileName)
		{
			IsFolderExist(_folderPath);

			foreach(var filePath in GetFilePathArray(_folderPath))
			{
				var fileName = GetFileName(filePath);

				if(string.Equals(fileName,_fileName))
				{
					return filePath;
				}
			}

			foreach(var folderPath in GetFolderPathArray(_folderPath))
			{
				var result = SearchFileInFolder(folderPath,_fileName);

				if(result != null)
				{
					return result;
				}
			}

			return null;
		}

		public static IEnumerable<string> SearchExtensionInFolder(string _folderPath,string _extension)
		{
			IsFolderExist(_folderPath);

			foreach(var filePath in GetFilePathArray(_folderPath,_extension))
			{
				yield return filePath;
			}

			foreach(var folderPath in GetFolderPathArray(_folderPath))
			{
				foreach(var filePath in SearchExtensionInFolder(folderPath,_extension))
				{
					yield return filePath;
				}
			}
		}
	}
}