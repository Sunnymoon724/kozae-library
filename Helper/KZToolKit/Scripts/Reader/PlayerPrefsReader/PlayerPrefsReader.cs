using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using KZLib.Utilities;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace KZLib.ToolKits
{
	public class PlayerPrefsReader
	{
		public static string[] LoadPlayerPrefsKeyArray(string companyName,string productName)
		{
			var resultList = new List<string>();

			if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var prefsPath = @$"SOFTWARE\Unity\UnityEditor\{companyName}\{productName}";

				using var registryKey = Registry.CurrentUser.OpenSubKey(prefsPath);

				if(registryKey != null)
				{
					foreach(var key in registryKey.GetValueNames())
					{
						var keyName = key[..key.LastIndexOf("_h",StringComparison.Ordinal)];

						resultList.Add(keyName);
					}
				}
			}
			else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				var inValidArray = new char[] { '"', '\\', '*', '/', ':', '<', '>', '?', '|' };

				var prefsPath = @$"Library/Preferences/unity.{_MakeValidFileName(companyName,inValidArray)}.{_MakeValidFileName(productName,inValidArray)}.plist";

				if(FileUtility.IsFileExist(prefsPath))
				{
					var cmdStr = string.Format(@"-p '{0}'", prefsPath.Replace("\"","\\\"").Replace("'","\\'").Replace("`","\\`"));

					var startInfo = new ProcessStartInfo
					{
						FileName = "plutil",
						Arguments = cmdStr,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						CreateNoWindow = true,
					};

					using Process process = new() { StartInfo = startInfo };

					process.Start();

					var output = process.StandardOutput.ReadToEnd();
					var error = process.StandardError.ReadToEnd();

					process.WaitForExit();

					if(!string.IsNullOrEmpty(error))
					{
						throw new Exception($"Error: {error}");
					}

					var matchCollection = Regex.Matches(output,@"(?: "")(.*)(?:"" =>.*)");

					for(int i=0;i<matchCollection.Count;i++)
					{
						var match = matchCollection[i];
						var keyName = match.Groups[1].Value;

						resultList.Add(keyName);
					}
				}
			}

			return resultList.ToArray();
		}

		private static string _MakeValidFileName(string fileName,char[] inValidCharArray)
		{
			var normalizedName = fileName.Trim().Normalize(NormalizationForm.FormD);
			var builder = new StringBuilder();

			foreach(var character in normalizedName)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);

				if(unicodeCategory == UnicodeCategory.NonSpacingMark)
				{
					continue;
				}

				if(Array.IndexOf(inValidCharArray,character) >= 0)
				{
					builder.Append('_');
				}
				else
				{
					builder.Append(character);
				}
			}

			return builder.ToString().Normalize(NormalizationForm.FormC);
		}
	}
}