namespace KZConsole
{
	internal class Utility
	{
		internal static string RemovePlusHeader(string text)
		{
			return text.TrimStart('+');
		}
	}
}