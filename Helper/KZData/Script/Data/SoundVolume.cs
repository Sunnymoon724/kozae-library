using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace KZLib.KZData
{
	public struct SoundVolume : IEquatable<SoundVolume>,IFormattable
	{
		public float level;
		public bool mute;

		private static readonly SoundVolume zeroSoundVolume = new SoundVolume(0.0f,true);
		private static readonly SoundVolume minSoundVolume = new SoundVolume(0.1f,true);
		private static readonly SoundVolume maxSoundVolume = new SoundVolume(1.0f,true);

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
			return ToString("",null);
		}

		public string ToString(string format)
		{
			return ToString(format,null);
		}

		public string ToString(string format,IFormatProvider? formatProvider)
		{
			if(string.IsNullOrEmpty(format))
			{
				format = "F2";
			}

			formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;

			return $"{level.ToString(format,formatProvider)} - Mute : {(mute ? "O" : "X")}";
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(level,mute);
		}

		public override bool Equals(object other)
		{
			if(!(other is SoundVolume))
			{
				return false;
			}

			return Equals((SoundVolume)other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SoundVolume other)
		{
			return level == other.level && mute == other.mute;
		}

		public void Toggle()
		{
			mute = !mute;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator +(SoundVolume left,SoundVolume right)
		{
			return new SoundVolume(left.level+right.level,left.mute || right.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator -(SoundVolume left,SoundVolume right)
		{
			return new SoundVolume(left.level-right.level,left.mute || right.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator *(SoundVolume left,SoundVolume right)
		{
			return new SoundVolume(left.level*right.level,left.mute || right.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator /(SoundVolume left,SoundVolume right)
		{
			return new SoundVolume(left.level/right.level,left.mute || right.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator *(SoundVolume left,float right)
		{
			return new SoundVolume(left.level*right,left.mute);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoundVolume operator *(float left,SoundVolume right)
		{
			return new SoundVolume(right.level*left,right.mute);
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
	}
}