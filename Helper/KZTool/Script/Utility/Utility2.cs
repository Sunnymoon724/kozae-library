using System;
using System.Collections.Generic;
using System.IO;

namespace KZLib.KZTool
{
	internal static partial class Utility
	{
		internal static void ValidRange(string sheetName,int index,int count)
		{
			if(index < 0 || index >= count)
			{
				throw new IndexOutOfRangeException($"{index} is out of range in {sheetName}");
			}
		}
	}
}