
namespace KZLib.Data
{
	public sealed class Version
	{
		private readonly string _value = string.Empty;

		public Version(string value)
		{
			_value = value;
		}

		public override string ToString()
		{
			return _value;
		}

		public static implicit operator string(Version instance)
		{
			return instance.ToString();
		}
	}
}