using System;
using System.IO;
using System.IO.Compression;

namespace KZLib.Utility
{
	public static partial class FileUtility
	{
		public static byte[] CompressBytes(byte[] _bytes)
		{
			if(_bytes == null || _bytes.Length == 0)
			{
				throw new NullReferenceException("Bytes is null or empty");
			}

			using var memoryStream = new MemoryStream();

			using(var archive = new ZipArchive(memoryStream,ZipArchiveMode.Create,true))
			{
				var entry = archive.CreateEntry("data.bin",CompressionLevel.Optimal);

				using var entryStream = entry.Open();

				entryStream.Write(_bytes,0,_bytes.Length);
			}

			return memoryStream.ToArray();
		}

		public static byte[] CompressZip(string _sourcePath)
		{
			if(IsFilePath(_sourcePath))
			{
				return CompressBytes(ReadFileToBytes(_sourcePath));
			}
			else
			{
				using var memoryStream = new MemoryStream();

				using(var archive = new ZipArchive(memoryStream,ZipArchiveMode.Create,true))
				{
					foreach(var filePath in Directory.GetFiles(_sourcePath,"*.*",SearchOption.AllDirectories))
					{
						var relativePath = Path.GetRelativePath(_sourcePath,filePath);
						var entry = archive.CreateEntry(relativePath,CompressionLevel.Optimal);

						using var entryStream = entry.Open();
						using var fileStream = File.OpenRead(filePath);

						fileStream.CopyTo(entryStream);
					}
				}

				return memoryStream.ToArray();
			}
		}

		public static void CompressZip(string _sourcePath,string _destinationPath)
		{
			IsPathExist(_destinationPath);

			var extension = GetExtension(_destinationPath);

			if(!string.Equals(extension,".zip"))
			{
				throw new NotSupportedException($"Not supported extension. [{_destinationPath}]");
			}

			if(string.IsNullOrEmpty(extension))
			{
				_destinationPath = $"{_destinationPath}.zip";
			}

			IsPathExist(_sourcePath);

			var compress = CompressZip(_sourcePath) ?? throw new NullReferenceException($"Compress is failed. {_sourcePath}");

			//? destinationPath == unique file path
			var destinationPath = GetUniquePath(_destinationPath);

			WriteByteToFile(destinationPath,compress);
		}

		public static byte[] DecompressBytes(byte[] _bytes)
		{
			if(_bytes == null || _bytes.Length == 0)
			{
				throw new NullReferenceException("Bytes is null or empty");
			}

			using var compressedStream = new MemoryStream(_bytes);
			using var archive = new ZipArchive(compressedStream,ZipArchiveMode.Read);

			var entry = archive.Entries[0];

			using var entryStream = entry.Open();
			using var memoryStream = new MemoryStream();

			entryStream.CopyTo(memoryStream);

			return memoryStream.ToArray();
		}

		public static void DecompressZip(string _sourcePath,string _destinationPath)
		{
			IsPathExist(_destinationPath);

			var destinationExtension = GetExtension(_destinationPath);

			if(!string.IsNullOrEmpty(destinationExtension))
			{
				throw new NullReferenceException($"{_destinationPath} is not folder path.");
			}

			IsFileExist(_sourcePath);

			var extension = GetExtension(_sourcePath);

			if(!string.Equals(extension,".zip"))
			{
				throw new NotSupportedException($"Not supported extension. [{_sourcePath}]");
			}

			//? destinationPath == unique folder path
			var destinationPath = GetUniquePath(_destinationPath);

			CreateFolder(destinationPath);

			using var zipStream = new FileStream(_sourcePath,FileMode.Open);
			using var archive = new ZipArchive(zipStream,ZipArchiveMode.Read);

			foreach(var entry in archive.Entries)
			{
				var fullPath = Path.Combine(destinationPath,entry.FullName);

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