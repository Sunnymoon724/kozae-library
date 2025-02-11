using System;
using System.IO;

namespace KZLib.KZData
{
	/// <summary>
	/// project folder path
	/// </summary>
	public readonly struct Route
	{
		private const string c_asset_header = "Assets";

		private readonly string m_localPath;
		private readonly string m_extension;

		public string Extension => m_extension;

		public Route(string header,string body,string extension = "") : this(Path.Combine($"{header}",$"{body}{(string.IsNullOrEmpty(extension) ? "" : $".{extension}")}")) { }

		public Route(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("Path cannot be null or empty.");
			}

			var projectPath = Directory.GetCurrentDirectory();

			if(path.Contains(c_asset_header))
			{
				if(!Path.IsPathRooted(path))
				{
					path = Path.Combine(projectPath,path);
				}
			}
			else
			{
				if(Path.IsPathRooted(path))
				{
					throw new InvalidDataException("Path is not in the project.");
				}

				path = Path.Combine(projectPath,c_asset_header,path);
			}

			path = Path.GetFullPath(path);

			var localPath = path.Replace($"{Path.Combine(projectPath,c_asset_header)}{Path.DirectorySeparatorChar}","");

			m_localPath = Path.Combine(Path.GetDirectoryName(localPath),Path.GetFileNameWithoutExtension(localPath).TrimEnd('.'));
			m_extension = Path.GetExtension(localPath).TrimStart('.');
		}

		public string AssetsPath => Path.Combine(c_asset_header,LocalPath);
		public string LocalPath => $"{m_localPath}{(string.IsNullOrEmpty(m_extension) ? "" : $".{m_extension}")}";

		public string AbsolutePath => Path.GetFullPath(AssetsPath);
	}
}