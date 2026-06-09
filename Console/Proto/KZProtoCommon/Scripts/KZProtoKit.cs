namespace KZConsole
{
	public static class KZProtoKit
	{
		private const string c_embeddedProtoFileSuffix = "Proto.cs";

		public static bool TryGetEmbeddedProtoName(string embeddedProtoFileName,out string protoName)
		{
			if(!embeddedProtoFileName.EndsWith(c_embeddedProtoFileSuffix))
			{
				protoName = string.Empty;

				return false;
			}

			protoName = embeddedProtoFileName[..^c_embeddedProtoFileSuffix.Length];

			return protoName.Length > 0;
		}

		public static string TrimProtoName(string sheetName)
		{
			return sheetName.TrimStart(ProtoGlobal.SheetPrefix);
		}

		public static string ToProtoSheetName(string protoName)
		{
			return $"{ProtoGlobal.SheetPrefix}{protoName}";
		}
	}
}