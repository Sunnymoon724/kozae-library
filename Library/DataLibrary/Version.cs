
namespace KZLib.Data
{
	public sealed class Version
	{
		private readonly string m_Value = string.Empty;

		public Version(string _value)
		{
			m_Value = _value;
		}

		public override string ToString()
		{
			return m_Value;
		}

		public static implicit operator string(Version _instance)
		{
			return _instance.ToString();
		}
	}
}