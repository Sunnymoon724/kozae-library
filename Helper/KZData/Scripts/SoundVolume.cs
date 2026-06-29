using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using MemoryPack;

namespace KZLib.Data
{
	/// <summary>
	/// Value type representing volume level (0.0 - 1.0) and mute state.
	/// When muted, level is preserved and restored on unMute.
	/// </summary>
	[MemoryPackable]
	public partial struct SoundVolume : IEquatable<SoundVolume>,IFormattable
	{
		private const float c_levelTolerance = 0.005f;
		private const NumberStyles c_levelNumberStyles = NumberStyles.Float | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent;

		/// <summary>Volume level (0.0 - 1.0, clamped on construction).</summary>
		public float level;

		/// <summary>Whether the channel is muted.</summary>
		public bool mute;

		private static readonly SoundVolume zeroSoundVolume	= new(0.0f,true);
		private static readonly SoundVolume minSoundVolume	= new(0.1f,false);
		private static readonly SoundVolume maxSoundVolume	= new(1.0f,false);

		/// <summary>Level 0 with mute enabled (fully silent preset).</summary>
		public static SoundVolume zero	=> zeroSoundVolume;

		/// <summary>Minimum audible level (0.1).</summary>
		public static SoundVolume min	=> minSoundVolume;

		/// <summary>Maximum level (1.0).</summary>
		public static SoundVolume max	=> maxSoundVolume;

		/// <summary>
		/// Creates an instance with the given level and mute state.
		/// level is clamped to the 0.0 - 1.0 range.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SoundVolume(float level,bool mute)
		{
			this.level = Math.Clamp(level,0.0f,1.0f);
			this.mute = mute;
		}

		/// <summary>Updates level and mute state. level is clamped to the 0.0 - 1.0 range.</summary>
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

		/// <summary>
		/// Returns a string in the format <c>level : {level}, mute : {mute}</c>. Default level format is F2.
		/// </summary>
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
			return HashCode.Combine(MathF.Round(level,2),mute);
		}

		public override bool Equals(object? other)
		{
			return other is SoundVolume volume && Equals(volume);
		}

		/// <summary>Returns true when both level and mute match.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SoundVolume volume)
		{
			return MathF.Abs(level-volume.level) < c_levelTolerance && mute == volume.mute;
		}

		/// <summary>Toggles mute state without changing level.</summary>
		public void Toggle()
		{
			mute = !mute;
		}

		/// <summary>Adds to level while preserving mute state.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator +(SoundVolume lhs,float rhs)
		{
			return new SoundVolume(lhs.level+rhs,lhs.mute);
		}

		/// <summary>Subtracts from level while preserving mute state.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator -(SoundVolume lhs,float rhs)
		{
			return new SoundVolume(lhs.level-rhs,lhs.mute);
		}

		/// <summary>Multiplies level while preserving mute state.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator *(SoundVolume lhs,float rhs)
		{
			return new SoundVolume(lhs.level*rhs,lhs.mute);
		}

		/// <summary>Divides level while preserving mute state. Throws if rhs is zero.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator /(SoundVolume lhs,float rhs)
		{
			if(rhs == 0.0f)
			{
				throw new DivideByZeroException("Cannot divide SoundVolume level by zero.");
			}

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
			if(!_TryParseCore(value,provider,out var soundVolume))
			{
				throw new FormatException($"Invalid SoundVolume format in '{value.ToString()}'");
			}

			return soundVolume;
		}

		public static SoundVolume Parse(string value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Parses a string produced by <see cref="ToString()"/> into a <see cref="SoundVolume"/>.
		/// Negative levels are clamped to 0.0 via construction.
		/// </summary>
		public static SoundVolume Parse(string value,IFormatProvider provider)
		{
			if(!_TryParseCore(value.AsSpan(),provider,out var soundVolume))
			{
				throw new FormatException($"Invalid SoundVolume format in '{value}'");
			}

			return soundVolume;
		}

		public static bool TryParse(ReadOnlySpan<char> value,out SoundVolume soundVolume)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out soundVolume);
		}

		public static bool TryParse(ReadOnlySpan<char> value,IFormatProvider provider,out SoundVolume soundVolume)
		{
			return _TryParseCore(value,provider,out soundVolume);
		}

		public static bool TryParse(string value,out SoundVolume soundVolume)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out soundVolume);
		}

		public static bool TryParse(string value,IFormatProvider provider,out SoundVolume soundVolume)
		{
			return _TryParseCore(value.AsSpan(),provider,out soundVolume);
		}

		private static bool _TryParseCore(ReadOnlySpan<char> value,IFormatProvider provider,out SoundVolume soundVolume)
		{
			soundVolume = default;

			var span = value.Trim();

			if(span.IsEmpty)
			{
				return false;
			}

			if(!_TryFindToken(ref span,"level"))
			{
				return false;
			}

			if(!_TryConsumeSeparator(ref span))
			{
				return false;
			}

			var commaIndex = span.IndexOf(',');

			if(commaIndex <= 0)
			{
				return false;
			}

			var levelSpan = span[..commaIndex].Trim();

			if(!float.TryParse(levelSpan,c_levelNumberStyles,provider,out var level))
			{
				return false;
			}

			span = span[(commaIndex+1)..].TrimStart();

			if(!_TryFindToken(ref span,"mute"))
			{
				return false;
			}

			if(!_TryConsumeSeparator(ref span))
			{
				return false;
			}

			if(!_TryParseBool(span.Trim(),out var mute))
			{
				return false;
			}

			soundVolume = new SoundVolume(level,mute);

			return true;
		}

		private static bool _TryFindToken(ref ReadOnlySpan<char> span,ReadOnlySpan<char> token)
		{
			var index = span.IndexOf(token,StringComparison.OrdinalIgnoreCase);

			if(index < 0)
			{
				return false;
			}

			span = span[(index+token.Length)..].TrimStart();

			return true;
		}

		private static bool _TryConsumeSeparator(ref ReadOnlySpan<char> span)
		{
			if(span.IsEmpty || span[0] != ':')
			{
				return false;
			}

			span = span[1..].TrimStart();

			return true;
		}

		private static bool _TryParseBool(ReadOnlySpan<char> span,out bool value)
		{
			if(span.Equals("true",StringComparison.OrdinalIgnoreCase))
			{
				value = true;

				return true;
			}

			if(span.Equals("false",StringComparison.OrdinalIgnoreCase))
			{
				value = false;

				return true;
			}

			value = default;

			return false;
		}
	}
}
