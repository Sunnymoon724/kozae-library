using System.Collections.Generic;

namespace KZConsole
{
	public static class ProtoGlobal
	{
		public const char SheetPrefix = '+';

		public const string BranchColumnScheme = "%Branch";
		public const string BranchSheetName = "Branch";

		public const int SchemeRowIndex = 0;
		public const int TypeRowIndex = 1;
		public const int CommentRowIndex = 2;
		public const int DataRowStartIndex = 3;

		public const int MainSheetIndex = 0;
		public const int SubSheetStartIndex = 1;

		public const int BranchMergeStartColumnIndex = 0;
		public const int BranchNameRowIndex = 0;
		public const int BranchStateRowIndex = 1;
		public const int BranchDataStartIndex = 1;

		public static readonly HashSet<string> ExcludeFileNameHashSet =
		[
			"Enum",
			"Branch",
		];
	}
}
