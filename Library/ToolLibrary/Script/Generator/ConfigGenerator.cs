using System.Text;
using KZLib.Utility;

namespace KZLib.Tool
{
	public class ConfigGenerator : DataGenerator
	{
		private struct ConfigScheme
		{
			public string Name { get; private set; }
			public string Type { get; private set; }
			public bool Writable { get; private set; }
			public string Default { get; private set; }
			public string Comment { get; private set; }
		}

		public ConfigGenerator(string _templatePath) : base(_templatePath) { }

		public override bool Generate(string _filePath,string _outputFolderPath)
		{
			base.Generate(_filePath,_outputFolderPath);

			var builder = new StringBuilder();
			var sheetName = FileUtility.GetFileName(_filePath);

			foreach(var scheme in ParseScheme<ConfigScheme>(sheetName,_filePath))
			{
				if(!scheme.Writable)
				{
					continue;
				}

				builder.Append($"// {scheme.Comment}\n");
				builder.Append($"public {scheme.Type} {scheme.Name} {{ get; private set; }} = {scheme.Default};\n");
			}

			if(builder.Length <= 0)
			{
				return false;
			}

			if(builder[^1] == '\n')
			{
				builder.Length--;
			}

			var template = m_TemplateText;

			template = template.Replace("$ClassName",sheetName);
			template = template.Replace("$PropertyFields",builder.ToString());

			FileUtility.WriteTextToFile(_outputFolderPath,template);

			return true;
		}
	}
}