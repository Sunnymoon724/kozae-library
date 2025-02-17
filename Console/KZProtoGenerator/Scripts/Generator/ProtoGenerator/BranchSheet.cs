using KZLib.KZTool;

namespace KZConsole.KZProto
{
	public class BranchSheet(string branchName,string branchFilePath)
	{
		private const string c_sheet_name = "Branch";

		private readonly Dictionary<string,bool> m_branchStateDict = [];

		public void GenerateBranch()
		{
			var excelReader = new ExcelReader(branchFilePath);

			if(!excelReader.IsExistSheetName(c_sheet_name))
			{
				throw new NullReferenceException($"{c_sheet_name} is not exist in {branchFilePath}.");
			}

			var schemeArray = excelReader.GetSchemeArray(c_sheet_name);

			if(schemeArray.Length == 0 || !schemeArray.Contains(branchName))
			{
				var header = string.Join("/",schemeArray);

				throw new NullReferenceException($"{branchName} is not exist in {header}. [{branchFilePath}]");
			}

			m_branchStateDict.Clear();

			var branchJaggedArray = excelReader.MergeCellArrayInColumns(c_sheet_name,Global.EXCEL_SCHEME_INDEX,Array.IndexOf(schemeArray,branchName));
			var branchArray = branchJaggedArray[Global.EXCEL_SCHEME_INDEX];

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
				throw new SheetConvertException($"{branch} not exist.",filePath,sheetName,line);
			}

			return result;
		}
	}
}