using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace KZConsole
{
	public class PaletteGenerator
	{
		private const int c_width = 256;
		private const int c_height = 1;
		private const int c_length = 8;

		private Rgba32 m_defaultColor = new(75,75,75,255);

		public void GeneratorPaletteImage(List<Color> colorList,string filePath)
		{
			var image = new Image<Rgba32>(c_width,c_height);
			var colorArray = new Rgba32[c_width*c_height];

			var dummyIndex = colorArray.Length-c_length;

			for(var i=0;i<dummyIndex;i++)
			{
				colorArray[i] = m_defaultColor;
			}

			for(var i=0;i<colorList.Count;i++)
			{
				colorArray[dummyIndex+i] = colorList[i];
			}

			colorArray[^1] = Color.White;

			for(var x=0;x<c_width; x++)
			{
				image[x,0] = colorArray[x];
			}

			image.SaveAsPng(filePath);
		}
	}
}