using System.IO;
using System.Text;
using Newtonsoft.Json;

public static partial class KZFileKit
{
	/// <summary>
	/// Creates the folder at path, or the parent folder when path is a file path.
	/// No-op when path is empty, the target folder path cannot be resolved, or the folder already exists.
	/// </summary>
	/// <param name="path">The absolute path of the file or folder.</param>
	public static void CreateFolder(string path)
	{
		if(!IsPathExist(path))
		{
			return;
		}

		var fullPath = IsFilePath(path) ? GetParentPath(path) : path;

		if(!IsPathExist(fullPath) || IsFolderExist(fullPath))
		{
			return;
		}

		Directory.CreateDirectory(fullPath);
	}

	/// <param name="filePath">The absolute path of the file.</param>
	public static void CreateFile(string filePath)
	{
		if(!IsPathExist(filePath))
		{
			return;
		}

		CreateFolder(filePath);

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

	/// <param name="folderPath">The absolute path of the folder.</param>
	/// <param name="fileName">The name of the file.</param>
	public static void WriteTextToFile(string folderPath,string fileName,string text)
	{
		WriteTextToFile(Path.Combine(folderPath,fileName),text);
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
}