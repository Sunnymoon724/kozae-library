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
		if(!IsValidPathString(path))
		{
			return;
		}

		var fullPath = HasFileExtension(path) ? GetParentPath(path) : path;

		if(!IsValidPathString(fullPath) || IsFolderExist(fullPath))
		{
			return;
		}

		Directory.CreateDirectory(fullPath);
	}

	/// <summary>
	/// Creates an empty file and its parent folders when the file does not exist.
	/// </summary>
	/// <param name="filePath">The absolute path of the file.</param>
	public static void CreateFile(string filePath)
	{
		if(!IsValidPathString(filePath))
		{
			return;
		}

		CreateFolder(filePath);

		if(!IsFileExist(filePath))
		{
			File.Create(filePath).Close();
		}
	}

	/// <summary>
	/// Writes bytes to a file, creating parent folders when needed.
	/// </summary>
	/// <param name="filePath">The absolute path of the file.</param>
	public static void WriteBytesToFile(string filePath,byte[] bytes)
	{
		if(!IsValidPathString(filePath))
		{
			return;
		}

		CreateFolder(filePath);

		File.WriteAllBytes(filePath,bytes);
	}

	/// <summary>
	/// Writes UTF-8 text to <paramref name="fileName"/> inside <paramref name="folderPath"/>.
	/// </summary>
	/// <param name="folderPath">The absolute path of the folder.</param>
	/// <param name="fileName">The name of the file.</param>
	public static void WriteTextToFile(string folderPath,string fileName,string text)
	{
		WriteTextToFile(Path.Combine(folderPath,fileName),text);
	}

	/// <summary>
	/// Writes UTF-8 text to a file, creating parent folders when needed.
	/// </summary>
	/// <param name="filePath">The absolute path of the file.</param>
	public static void WriteTextToFile(string filePath,string text)
	{
		WriteTextToFile(filePath,text,Encoding.UTF8);
	}

	/// <summary>
	/// Writes text to a file with the given <paramref name="encoding"/>, creating parent folders when needed.
	/// </summary>
	/// <param name="filePath">The absolute path of the file.</param>
	public static void WriteTextToFile(string filePath,string text,Encoding encoding)
	{
		if(!IsValidPathString(filePath))
		{
			return;
		}

		CreateFolder(filePath);

		File.WriteAllText(filePath,text,encoding);
	}

	/// <summary>
	/// Serializes <paramref name="target"/> as JSON and writes it to a UTF-8 text file.
	/// </summary>
	/// <param name="filePath">The absolute path of the file.</param>
	public static void WriteJsonToFile<TObject>(string filePath,TObject target)
	{
		if(!IsValidPathString(filePath))
		{
			return;
		}

		WriteTextToFile(filePath,JsonConvert.SerializeObject(target));
	}
}
