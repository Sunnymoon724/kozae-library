using System.Collections.Generic;
using KZLib.Utility;

namespace KZLib.Tool
{
	public abstract class DataGenerator
	{
		protected readonly string m_TemplateText = null;

		public DataGenerator(string _templatePath)
		{
			m_TemplateText = FileUtility.ReadFileToText(_templatePath);
		}

		public virtual bool Generate(string _filePath,string _outputFolderPath)
		{
			FileUtility.IsFileExist(_filePath);

			return true;
		}

		public bool GenerateAll(string _folderPath,string _outputFolderPath)
		{
			foreach(var filePath in FileUtility.GetExcelFileGroup(_folderPath))
			{
				if(!Generate(filePath,_outputFolderPath))
				{
					return false;
				}
			}

			return true;
		}

		protected IEnumerable<TData> ParseScheme<TData>(string _sheetName,string _filePath) where TData : struct
		{
			var reader = new ExcelReader(_filePath);

			return reader.Deserialize<TData>(_sheetName);
		}
	}
}