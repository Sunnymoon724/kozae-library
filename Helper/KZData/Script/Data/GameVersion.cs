using System;

namespace KZLib.KZData
{
	public sealed class GameVersion
	{
		private readonly string m_value = string.Empty;

		public GameVersion(string value)
		{
			m_value = value ?? throw new ArgumentNullException("value is null");
		}

		public override string ToString()
		{
			return $"GameVersion: {m_value}";
		}

		public static implicit operator string(GameVersion instance)
		{
			return instance.ToString() ?? string.Empty;
		}

		public int CompareTo(GameVersion other)
		{
			if(other == null)
			{
				return 1;
			}

			return string.Compare(m_value,other.m_value,StringComparison.OrdinalIgnoreCase);
		}
	}
}