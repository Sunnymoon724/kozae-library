using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace KZLib.KZData
{
	/// <summary>
	/// level -> 0.0 - 1.0
	/// </summary>
	public struct SoundVolume : IEquatable<SoundVolume>,IFormattable
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

			return $"level : {level.ToString(format,formatProvider)}, mute : {mute}";
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

		public void Toggle()
		{
			mute = !mute;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator +(SoundVolume a,float b)
		{
			return new SoundVolume(a.level+b,a.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator -(SoundVolume a,float b)
		{
			return new SoundVolume(a.level-b,a.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator *(SoundVolume a,float b)
		{
			return new SoundVolume(a.level*b,a.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator /(SoundVolume a,float b)
		{
			return new SoundVolume(a.level/b,a.mute);
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
	}
}