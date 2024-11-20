using System.IO;
using Newtonsoft.Json;

namespace KZLib.Utility
{
	public static partial class FileUtility
	{
		/// <summary>
		/// Create folder. (path is file ? create parent folder. : create folder)
		/// </summary>
		public static void CreateFolder(string _path)
		{
			IsPathExist(_path);

			// Path is file ? Get parent path. : Get path
			var folderPath = IsFilePath(_path) ? GetParentPath(_path) : _path;

			Directory.CreateDirectory(folderPath);
		}

		public static void CreateFile(string _filePath)
		{
			IsFileExist(_filePath);

			File.Create(_filePath).Close();
		}

		public static void WriteByteToFile(string _filePath,byte[] _bytes)
		{
			CreateFolder(_filePath);

			File.WriteAllBytes(_filePath,_bytes);
		}

		public static void WriteTextToFile(string _filePath,string _text)
		{
			CreateFolder(_filePath);

			File.WriteAllText(_filePath,_text);
		}

		public static void WriteJsonToFile<TObject>(string _filePath,TObject _object)
		{
			WriteTextToFile(_filePath,JsonConvert.SerializeObject(_object));
		}

		public static void CopyFile(string _sourcePath,string _destinationPath,bool _isOverride)
		{
			IsFileExist(_sourcePath);

			var fileName = GetFileName(_sourcePath);
			var destinationPath = PathCombine(_destinationPath,fileName);

			if(IsFileExist(destinationPath) && !_isOverride)
			{
				return;
			}

			WriteByteToFile(destinationPath,ReadFileToBytes(_sourcePath));
		}

		public static void CopyFolder(string _sourcePath,string _destinationPath,bool _isOverride)
		{
			IsFileExist(_sourcePath);

			CreateFolder(_destinationPath);

			foreach(var filePath in GetFilePathArray(_sourcePath,"*.*"))
			{
				CopyFile(filePath,_destinationPath,_isOverride);
			}
		}
	}
}