using System.IO;
using KZLib.Utility;

namespace KZLib.Tool
{
	public class ProtoGenerator : DataGenerator
	{
		private struct ConfigScheme
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public bool Writable { get; set; }
			public object Default { get; set; }
		}

		public ProtoGenerator(string _templatePath) : base(_templatePath) { }

		public override bool Generate(string _filePath,string _outputFolderPath)
		{
			var name = FileUtility.GetFileName(_filePath);

			var reader = new ExcelReader(_filePath);


			return true;
		}
	}
}