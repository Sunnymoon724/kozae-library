
using System.Runtime.InteropServices;
using System.Text;
using MessagePack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KZConsole
{
	internal class Utility
    {
		internal static bool IsPathExist(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				throw new NullReferenceException("Path is null");
			}

			return true;
		}

		internal static string GetFullPath(params string[] pathArray)
		{
			var path = Path.Combine(pathArray);

			IsPathExist(path);

			return Path.GetFullPath(path);
		}

		/// <summary>
		/// Create folder. (path is file ? create parent folder. : create folder)
		/// </summary>
		internal static void CreateFolder(string path)
		{
			IsPathExist(path);

			// Path is file ? Get parent path. : Get path
			var folderPath = (Path.HasExtension(path) ? Path.GetDirectoryName(path) : path) ?? throw new NullReferenceException($"Parent path not exist. [{path}]");

			Directory.CreateDirectory(folderPath);
		}

		internal static void WriteTextToFile(string filePath,string text)
		{
			CreateFolder(filePath);

			File.WriteAllText(filePath,text,Encoding.UTF8);
		}

		internal static void WriteBytesToFile(string filePath,byte[] bytes)
		{
			CreateFolder(filePath);

			File.WriteAllBytes(filePath,bytes);
		}

		internal static string RemovePlusHeader(string text)
		{
			return text.TrimStart('+');
		}
	}
}