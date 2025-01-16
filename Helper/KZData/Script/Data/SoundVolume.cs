using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace KZLib.KZData
{
	/// <summary>
	/// level -> 0.0 - 1.0
	/// </summary>
	public struct SoundVolume : IEquatable<SoundVolume>,IFormattable,IComparable,IComparable<SoundVolume>
	{
		public float level;
		public bool mute;

		private static readonly SoundVolume zeroSoundVolume = new SoundVolume(0.0f,true);
		private static readonly SoundVolume minSoundVolume = new SoundVolume(0.1f,false);
		private static readonly SoundVolume maxSoundVolume = new SoundVolume(1.0f,false);

		public static SoundVolume zero => zeroSoundVolume;
		public static SoundVolume min => minSoundVolume;
		public static SoundVolume max => maxSoundVolume;

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

		public string ToString(string format,IFormatProvider? formatProvider)
		{
			if(string.IsNullOrEmpty(format))
			{
				format = "F2";
			}

			formatProvider ??= CultureInfo.InvariantCulture;

			return $"{level.ToString(format,formatProvider)} - Mute : {(mute ? "O" : "X")}";
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(level,mute);
		}

		public override bool Equals(object other)
		{
			return other is SoundVolume volume && Equals(volume);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SoundVolume volume)
		{
			return level == volume.level && mute == volume.mute;
		}

		public int CompareTo(object other)
		{
			if(other is SoundVolume volume)
			{
				return CompareTo(volume);
			}

			throw new ArgumentException($"{other} is not a SoundVolume");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(SoundVolume volume)
		{
			return level.CompareTo(volume.level);
		}

		public void Toggle()
		{
			mute = !mute;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator +(SoundVolume left,float right)
		{
			return new SoundVolume(left.level+right,left.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator -(SoundVolume left,float right)
		{
			return new SoundVolume(left.level-right,left.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator *(SoundVolume left,float right)
		{
			return new SoundVolume(left.level*right,left.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator /(SoundVolume left,float right)
		{
			return new SoundVolume(left.level/right,left.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SoundVolume left,SoundVolume right)
		{
			return left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SoundVolume left,SoundVolume right)
		{
			return !left.Equals(right);
		}

		public static SoundVolume Parse(string value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static SoundVolume Parse(string value,IFormatProvider provider)
		{
			return Parse(value.AsSpan(),provider);
		}

		public static SoundVolume Parse(ReadOnlySpan<char> value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static SoundVolume Parse(ReadOnlySpan<char> value,IFormatProvider provider)
		{
			//? $"AAA - Mute : B";

			var index1 = value.IndexOf('-');

			if(index1 == -1)
			{
				throw new FormatException($"Invalid format in {value.ToString()}");
			}

			var levelPart = value[..index1];

			if(!float.TryParse(levelPart,NumberStyles.Float,provider,out var level))
			{
				throw new FormatException($"Invalid level format in {levelPart.ToString()}");
			}

			var index2 = value.IndexOf(':');

			if(index2 == -1)
			{
				throw new FormatException("Invalid format for ScreenResolution.");
			}

			var mutePart = value[(index2+1)..].Trim();

			if(mutePart != "O" && mutePart != "X")
			{
				throw new FormatException($"Invalid mute format in {mutePart.ToString()}");
			}

			var mute = mutePart == "O";

			return new SoundVolume(level,mute);
		}

		public static bool TryParse(string value,out SoundVolume volume)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out volume);
		}

		public static bool TryParse(string value,IFormatProvider provider,out SoundVolume volume)
		{
			return TryParse(value.AsSpan(),provider,out volume);
		}

		public static bool TryParse(ReadOnlySpan<char> value,out SoundVolume volume)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out volume);
		}

		public static bool TryParse(ReadOnlySpan<char> value,IFormatProvider provider,out SoundVolume volume)
		{
			try
			{
				volume = Parse(value,provider);

				return true;
			}
			catch
			{
				volume = default;

				return false;
			}
		}
	}
}