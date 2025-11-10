using System.IO;
using UnityEngine;

namespace KZConsole.KZUtility
{
    public struct Global
	{
		public static readonly string[] EXCEPTION_FILE_NAME_ARRAY = [BRANCH,ENUM];
		
		public static readonly string NEW_LINE = Environment.NewLine;

		public const string BRANCH = "Branch";
		public const string ENUM = "Enum";
		public const string DATA_FILE_NAME = "KZData.dll";

		public const char PLUS_MARK = '+';
	}
}