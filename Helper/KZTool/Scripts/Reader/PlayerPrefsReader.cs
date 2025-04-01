using System;
using System.Collections.Generic;

#if WINDOWS

using Microsoft.Win32;

#elif MACOS

using System.IO;
using Claunia.PropertyList;

#endif

namespace KZLib.KZTool
{
	public class PlayerPrefsReader
	{
		public static string[] LoadPlayerPrefsKey(string companyName,string productName)
		{
			var resultList = new List<string>();
#if WINDOWS
			var prefsPath = @$"Software\Unity\UnityEditor\{companyName}\{productName}";

			using var registryKey = Registry.CurrentUser.OpenSubKey(prefsPath);

			if(registryKey != null)
			{
				foreach(var key in registryKey.GetValueNames())
				{
					if(key.StartsWith("unity.") || key.StartsWith("UnityGraphicsQuality"))
					{
						continue;
					}

					resultList.Add(key[..key.LastIndexOf("_h",StringComparison.Ordinal)]);
				}
			}
#elif MACOS

			var prefsPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library/Preferences"),$"unity.{companyName}.{productName}.plist");
			var dictionary = PropertyListParser.Parse(new FileInfo(prefsPath)) as NSDictionary ?? throw new InvalidOperationException($"{prefsPath} is not valid");

			foreach(var pair in dictionary)
			{
				resultList.Add(pair.Key);
			}
#endif
			return resultList.ToArray();
		}
	}
}