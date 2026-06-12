using System;
using System.IO;

public static partial class KZFileKit
{
	/// <summary>
	/// Reads the entire file as text, or returns an empty string when the file does not exist.
	/// </summary>
	/// <param name="filePath">The absolute file path.</param>
	public static string ReadTextFromFile(string filePath)
	{
		if(!IsFileExist(filePath))
		{
			return string.Empty;
		}

		return _ReadFile(filePath,File.ReadAllText);
	}

	/// <summary>
	/// Reads the entire file as bytes, or returns an empty array when the file does not exist.
	/// </summary>
	/// <param name="filePath">The absolute file path.</param>
	public static byte[] ReadBytesFromFile(string filePath)
	{
		if(!IsFileExist(filePath))
		{
			return Array.Empty<byte>();
		}

		return _ReadFile(filePath,File.ReadAllBytes);
	}

	/// <summary>
	/// Returns the file size in bytes, or 0 when the file does not exist.
	/// </summary>
	public static long GetFileSizeByte(string filePath)
	{
		return !IsFileExist(filePath) ? 0L : new FileInfo(filePath).Length;
	}

	/// <summary>
	/// Returns the file size in kilobytes (integer division of bytes).
	/// </summary>
	public static long GetFileSizeKB(string filePath)
	{
		return (long)(GetFileSizeByte(filePath)/(double)c_kiloByte);
	}

	/// <summary>
	/// Returns the file size in megabytes (integer division of bytes).
	/// </summary>
	public static long GetFileSizeMB(string filePath)
	{
		return (long)(GetFileSizeByte(filePath)/(double)c_megaByte);
	}

	private static TRead _ReadFile<TRead>(string filePath,Func<string,TRead> onRead)
	{
		return onRead(filePath);
	}
}
