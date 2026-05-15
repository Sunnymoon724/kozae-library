using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MemoryPack;

namespace KZLib.Data
{
	/// <summary>
	/// level -> 0.0 - 1.0
	/// </summary>
	[MemoryPackable]
	public partial struct SoundVolume : IEquatable<SoundVolume>,IFormattable
	{
		public float level;
		public bool mute;

		private static readonly SoundVolume zeroSoundVolume	= new(0.0f,true);
		private static readonly SoundVolume minSoundVolume	= new(0.1f,false);
		private static readonly SoundVolume maxSoundVolume	= new(1.0f,false);

		public static SoundVolume zero	=> zeroSoundVolume;
		public static SoundVolume min	=> minSoundVolume;
		public static SoundVolume max	=> maxSoundVolume;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SoundVolume(float level,bool mute)
		{
			this.level = Math.Clamp(level,0.0f,1.0f);
			this.mute = mute;
		}

		public void Set(float newLevel,bool newMute)
		{
			level = Math.Clamp(newLevel,0.0f,1.0f);
			mute = newMute;
		}

		public override string ToString()
		{
			return ToString(string.Empty,CultureInfo.InvariantCulture);
		}

		public string ToString(string format)
		{
			return ToString(format,CultureInfo.InvariantCulture);
		}

		public string ToString(string? format,IFormatProvider? formatProvider)
		{
			if(string.IsNullOrEmpty(format))
			{
				format = "F2";
			}

			formatProvider ??= CultureInfo.InvariantCulture;

			return $"level : {level.ToString(format,formatProvider)}, mute : {mute}";
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(level,mute);
		}

		public override bool Equals(object? other)
		{
			return other is SoundVolume volume && Equals(volume);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SoundVolume volume)
		{
			return level == volume.level && mute == volume.mute;
		}

		public void Toggle()
		{
			mute = !mute;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator +(SoundVolume lhs,float rhs)
		{
			return new SoundVolume(lhs.level+rhs,lhs.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator -(SoundVolume lhs,float rhs)
		{
			return new SoundVolume(lhs.level-rhs,lhs.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator *(SoundVolume lhs,float rhs)
		{
			return new SoundVolume(lhs.level*rhs,lhs.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator /(SoundVolume lhs,float rhs)
		{
			return new SoundVolume(lhs.level/rhs,lhs.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SoundVolume lhs,SoundVolume rhs)
		{
			return lhs.Equals(rhs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SoundVolume lhs,SoundVolume rhs)
		{
			return !lhs.Equals(rhs);
		}

		public static SoundVolume Parse(ReadOnlySpan<char> value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static SoundVolume Parse(ReadOnlySpan<char> value,IFormatProvider provider)
		{
			return Parse(value.ToString(),provider);
		}

		public static SoundVolume Parse(string value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static SoundVolume Parse(string value,IFormatProvider provider)
		{
			var levelRegex = new Regex(@"level\s*:\s*(\d+(\.\d+)?)");
			var levelMatch = levelRegex.Match(value);

			if(!levelMatch.Success || !float.TryParse(levelMatch.Groups[1].Value,NumberStyles.AllowDecimalPoint,provider,out var level))
			{
				throw new FormatException($"Invalid level format in '{value}'");
			}

			var muteRegex = new Regex(@"mute\s*:\s*(true|false)",RegexOptions.IgnoreCase);
			var muteMatch = muteRegex.Match(value);

			if(!muteMatch.Success || !bool.TryParse(muteMatch.Groups[1].Value,out var mute))
			{
				throw new FormatException($"Invalid mute format in '{value}'");
			}

			return new SoundVolume(level,mute);
		}


		public static bool TryParse(ReadOnlySpan<char> value,out SoundVolume soundVolume)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out soundVolume);
		}

		public static bool TryParse(ReadOnlySpan<char> value,IFormatProvider provider,out SoundVolume soundVolume)
		{
			return TryParse(value.ToString(),provider,out soundVolume);
		}

		public static bool TryParse(string value,out SoundVolume soundVolume)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out soundVolume);
		}

		public static bool TryParse(string value,IFormatProvider provider,out SoundVolume soundVolume)
		{
			try
			{
				soundVolume = Parse(value,provider);

				return true;
			}
			catch
			{
				soundVolume = default;

				return false;
			}
		}
	}
}