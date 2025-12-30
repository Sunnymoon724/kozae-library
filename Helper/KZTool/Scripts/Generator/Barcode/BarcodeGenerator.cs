using System;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.Common;

public class BarcodeGenerator
{
	public static Texture2D GenerateCode128Barcode(string content,int width,int height,bool addQuietZone = true)
	{
		if(string.IsNullOrEmpty(content))
		{
			throw new NullReferenceException("Content is not null or empty.");
		}

		var barcodeWriter = new BarcodeWriter<Color32[]>
		{
			Format = BarcodeFormat.CODE_128,
			Options = new EncodingOptions
			{
				Width = width,
				Height = height,
				Margin = addQuietZone ? 10 : 0,
				PureBarcode = true
			}
		};

		var pixelArray = barcodeWriter.Write(content); 
		// var pixels = barcodeWriter.RenderAsColor32(data);

		var fixedPixelArray = new Color32[pixelArray.Length];

		for(var y=0;y<height;y++)
		{
			var srcRow = y*width;
			var dstRow = (height-1-y)*width; 

			Array.Copy(pixelArray,srcRow,fixedPixelArray,dstRow,width);
		}

		var resultTexture = new Texture2D(width,height,TextureFormat.RGBA32,false)
		{
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp
		};

		resultTexture.SetPixels32(fixedPixelArray);
		resultTexture.Apply();

		return resultTexture;
	}
}