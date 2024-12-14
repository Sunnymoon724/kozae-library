using System;
using UnityEngine;

namespace KZLib.KZUtility
{
	using Object = UnityEngine.Object;

	internal static class CommonUtility
	{
		internal static TObject CopyObject<TObject>(TObject item,Transform? parent) where TObject : Object
		{
			if(!item)
			{
				throw new NullReferenceException("Object is null");
			}

			var data = Object.Instantiate(item,parent);
			data.name = item.name;

			return data;
		}

		internal static void DestroyObject(Object item)
		{
			if(!item)
			{
				return;
			}

			if(Application.isPlaying)
			{
				Object.Destroy(item);
			}
			else
			{
				Object.DestroyImmediate(item);
			}
		}
	}
}