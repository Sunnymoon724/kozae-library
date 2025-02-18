using System.Globalization;
using KZLib.KZTool;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace KZConsole
{
	public class ColorReader
	{
		private readonly Dictionary<string,Color> m_hexColorDict = [];

		public Dictionary<string,List<Color>> ConvertColorListDict(string colorExcelFilePath)
		{
			var colorListDict = new Dictionary<string,List<Color>>();

			var sheetName = "+Color";
			var excelReader = new ExcelReader(colorExcelFilePath);

			var schemeArray = excelReader.GetSchemeArray(sheetName);

			var nameIndex = FindIndexGroup(schemeArray,"Name",sheetName).First();
			var colorIndexGroup = FindIndexGroup(schemeArray,"ColorArray",sheetName);

			var rowSize = excelReader.GetRowSize(sheetName);

			for(var i=2;i<rowSize;i++)
			{
				var cellArray = excelReader.GetCellArrayInRow(sheetName,i);

				// skip empty
				if(cellArray.Length == 0)
				{
					continue;
				}

				var name = cellArray[nameIndex];
				var colorList = new List<Color>();

				var startIndex = colorIndexGroup.First();

				foreach(var colorIndex in colorIndexGroup)
				{
					var color = string.IsNullOrEmpty(cellArray[colorIndex]) ? ConvertToColor(cellArray[startIndex]) : ConvertToColor(cellArray[colorIndex]);

					colorList.Add(color);
				}

				colorListDict.Add(name,colorList);
			}

			return colorListDict;
		}

		private static IEnumerable<int> FindIndexGroup(string[] schemeArray,string text,string sheetName)
		{
			for(var i=0;i<schemeArray.Length;i++)
			{
				var scheme = schemeArray[i];

				if(string.Equals(scheme,text))
				{
					yield return i;
				}
			}
		}

		public Color ConvertToColor(string hexCode)
		{
			hexCode = hexCode.Replace("#","");

			if(hexCode.Length == 6)
			{
				hexCode = $"{hexCode}FF";
			}

			if(m_hexColorDict.TryGetValue(hexCode,out var value))
			{
				return value;
			}

			var r = byte.Parse(hexCode.Substring(0,2),NumberStyles.HexNumber);
			var g = byte.Parse(hexCode.Substring(2,2),NumberStyles.HexNumber);
			var b = byte.Parse(hexCode.Substring(4,2),NumberStyles.HexNumber);
			var a = byte.Parse(hexCode.Substring(6,2),NumberStyles.HexNumber);

			return new Color(new Rgba32(r,g,b,a));
		}
	}
}