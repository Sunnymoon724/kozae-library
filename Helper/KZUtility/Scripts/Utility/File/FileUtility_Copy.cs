using System.IO;

namespace KZLib.KZUtility
{
	public static partial class FileUtility
	{
		/// <param name="sourcePath">The absolute path of the file.</param>
		/// <param name="_destinationPath">The absolute path of the folder.</param>
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

		/// <param name="sourcePath">The absolute path of the folder.</param>
		/// <param name="_destinationPath">The absolute path of the folder.</param>
		public static void CopyFolder(string sourceFolderPath,string destinationFolderPath,bool isOverride)
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
}