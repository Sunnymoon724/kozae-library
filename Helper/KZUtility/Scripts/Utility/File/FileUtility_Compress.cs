using System;
using System.IO;
using System.IO.Compression;

namespace KZLib.Utilities
{
	public static partial class FileUtility
	{
		public static byte[] CompressBytes(byte[] bytes)
		{
			if(bytes == null || bytes.Length == 0)
			{
				throw new InvalidDataException("Bytes is null or empty");
			}

			using var memoryStream = new MemoryStream();
			using var archive = new ZipArchive(memoryStream,ZipArchiveMode.Create,true);

			var entry = archive.CreateEntry("data.bin",CompressionLevel.Optimal);

			using var entryStream = entry.Open();

			entryStream.Write(bytes,0,bytes.Length);

			return memoryStream.ToArray();
		}

		/// <param name="sourcePath">The absolute path of the file or folder.</param>
		public static byte[] CompressZip(string sourcePath)
		{
			if(!IsPathExist(sourcePath))
			{
				return Array.Empty<byte>();
			}

			if(IsFilePath(sourcePath))
			{
				return CompressBytes(ReadFileToBytes(sourcePath));
			}
			else
			{
				using var memoryStream = new MemoryStream();
				using var archive = new ZipArchive(memoryStream,ZipArchiveMode.Create,true);

				foreach(var filePath in Directory.GetFiles(sourcePath,"*.*",SearchOption.AllDirectories))
				{
					var relativePath = Path.GetRelativePath(sourcePath,filePath);
					var entry = archive.CreateEntry(relativePath,CompressionLevel.Optimal);

					using var entryStream = entry.Open();
					using var fileStream = File.OpenRead(filePath);

					fileStream.CopyTo(entryStream);
				}

				return memoryStream.ToArray();
			}
		}

		/// <param name="sourcePath">The absolute path of the file or folder.</param>
		/// <param name="destinationPath">The absolute path of the file or folder.</param>
		public static void CompressZip(string sourcePath,string destinationPath)
		{
			if(!IsPathExist(destinationPath))
			{
				return;
			}

			var extension = GetExtension(destinationPath);

			if(!string.Equals(extension,".zip"))
			{
				throw new NotSupportedException($"Not supported extension. [{destinationPath}]");
			}

			if(string.IsNullOrEmpty(extension))
			{
				destinationPath = $"{destinationPath}.zip";
			}

			if(!IsPathExist(sourcePath))
			{
				return;
			}

			var compress = CompressZip(sourcePath);

			if(compress == null || compress.Length == 0)
			{
				throw new InvalidOperationException($"Compress is failed. {sourcePath}");
			}

			//? destinationPath == unique file path
			var uniquePath = _GetUniquePath(destinationPath);

			WriteByteToFile(uniquePath,compress);
		}

		public static byte[] DecompressBytes(byte[] bytes)
		{
			if(bytes == null || bytes.Length == 0)
			{
				throw new InvalidOperationException("Bytes is null or empty");
			}

			using var compressedStream = new MemoryStream(bytes);
			using var archive = new ZipArchive(compressedStream,ZipArchiveMode.Read);

			var entry = archive.Entries[0];

			using var entryStream = entry.Open();
			using var memoryStream = new MemoryStream();

			entryStream.CopyTo(memoryStream);

			return memoryStream.ToArray();
		}

		/// <param name="sourcePath">The absolute path of the file.</param>
		/// <param name="destinationPath">The absolute path of the folder.</param>
		public static void DecompressZip(string sourcePath,string destinationPath)
		{
			if(!IsPathExist(destinationPath))
			{
				return;
			}

			var destinationExtension = GetExtension(destinationPath);

			if(!string.IsNullOrEmpty(destinationExtension))
			{
				throw new InvalidDataException($"{destinationPath} is not folder path.");
			}

			if(!IsFileExist(sourcePath))
			{
				return;
			}

			var sourceExtension = GetExtension(sourcePath);

			if(!string.Equals(sourceExtension,".zip"))
			{
				throw new ArgumentException($"{sourcePath} is not zip file.");
			}

			//? destinationPath == unique folder path
			var uniquePath = _GetUniquePath(destinationPath);

			CreateFolder(uniquePath);

			using var zipStream = new FileStream(sourcePath,FileMode.Open);
			using var archive = new ZipArchive(zipStream,ZipArchiveMode.Read);

			foreach(var entry in archive.Entries)
			{
				var fullPath = Path.Combine(uniquePath,entry.FullName);

				if(!entry.FullName.EndsWith("/"))
				{
					CreateFolder(fullPath);

					using var entryStream = entry.Open();
					using var fileStream = new FileStream(fullPath,FileMode.Create);

					entryStream.CopyTo(fileStream);
				}
			}
		}
	}
}