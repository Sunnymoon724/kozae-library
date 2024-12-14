using System;

namespace KZLib.KZData
{
	public sealed class GameVersion
	{
		private readonly string _value = string.Empty;

		public GameVersion(string value)
		{
			_value = value ?? throw new ArgumentNullException("value is null");
		}

		public override string ToString()
		{
			return $"GameVersion: {_value}";
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

			return string.Compare(_value,other._value,StringComparison.OrdinalIgnoreCase);
		}
	}
}