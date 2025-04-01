using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace KZLib.KZUtility
{
	public static partial class FileUtility
	{
		/// <summary>
		/// Create folder. (path is file ? create parent folder. : create folder)
		/// </summary>
		/// <param name="path">The absolute path of the file or folder.</param>
		public static void CreateFolder(string path)
		{
			if(!IsPathExist(path))
			{
				return;
			}

			// Path is file ? Get parent path. : Get path
			var fullPath = IsFilePath(path) ? GetParentPath(path) : path;

			if(!IsFolderExist(fullPath))
			{
				Directory.CreateDirectory(fullPath);
			}
		}

		/// <param name="filePath">The absolute path of the file.</param>
		public static void CreateFile(string filePath)
		{
			if(!IsPathExist(filePath))
			{
				return;
			}

			if(!IsFileExist(filePath))
			{
				File.Create(filePath).Close();
			}
		}

		/// <param name="filePath">The absolute path of the file.</param>
		public static void WriteByteToFile(string filePath,byte[] bytes)
		{
			if(!IsPathExist(filePath))
			{
				return;
			}

			CreateFolder(filePath);

			File.WriteAllBytes(filePath,bytes);
		}

		/// <param name="filePath">The absolute path of the file.</param>
		public static void WriteTextToFile(string filePath,string text)
		{
			WriteTextToFile(filePath,text,Encoding.UTF8);
		}

		/// <param name="filePath">The absolute path of the file.</param>
		public static void WriteTextToFile(string filePath,string text,Encoding encoding)
		{
			if(!IsPathExist(filePath))
			{
				return;
			}

			CreateFolder(filePath);

			File.WriteAllText(filePath,text,encoding);
		}

		/// <param name="filePath">The absolute path of the file.</param>
		public static void WriteJsonToFile<TObject>(string filePath,TObject target)
		{
			if(!IsPathExist(filePath))
			{
				return;
			}

			WriteTextToFile(filePath,JsonConvert.SerializeObject(target));
		}

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

			WriteByteToFile(destinationFilePath,ReadFileToBytes(sourceFilePath));
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