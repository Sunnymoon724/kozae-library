using System;
using System.Collections.Generic;
using KZLib.ToolKits;
using KZLib.Utilities;

namespace KZConsole
{
	public class BranchGenerator(string branchName,string branchFilePath)
	{
		private readonly Dictionary<string,bool> m_branchStateDict = [];

		public void GenerateBranch()
		{
			var excelReader = new ExcelReader(branchFilePath);

			if(!excelReader.IsExistSheetName("Branch"))
			{
				throw new InvalidOperationException($"Branch is not exist in {branchFilePath}.");
			}

			var schemeArray = excelReader.FindSchemeArray("Branch");

			if(schemeArray.Length == 0 || Array.IndexOf(schemeArray, branchName) < 0)
			{
				var header = string.Join("/",schemeArray);

				throw new ArgumentException($"{branchName} is not exist in {header}. [{branchFilePath}]");
			}

			m_branchStateDict.Clear();

			var branchJaggedArray = excelReader.MergeCellArrayInColumns("Branch",0,Array.IndexOf(schemeArray,branchName));
			var branchArray = branchJaggedArray[0];

			for(int i=1;i<branchArray.Length;i++)
			{
				var branch = branchArray[i];

				if(string.IsNullOrEmpty(branch))
				{
					continue;
				}

				if(m_branchStateDict.ContainsKey(branch))
				{
					throw new ArgumentException($"{branch} is already exist. [overlap index = {i}]");
				}

				var branchState = branchJaggedArray[1][i];

				if(string.IsNullOrEmpty(branchState))
				{
					continue;
				}

				m_branchStateDict.Add(branch,bool.TryParse(branchState,out var result) && result);
			}
		}

		public bool IsIncludeRow(string branch,string filePath,string sheetName,int line)
		{
			if(string.IsNullOrEmpty(branch))
			{
				return false;
			}

			if(!m_branchStateDict.TryGetValue(branch,out var result))
			{
				throw new KZSheetException($"{branch} not exist.",filePath,sheetName,line);
			}

			return result;
		}
	}
}