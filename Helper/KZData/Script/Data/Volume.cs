using System;
using System.Globalization;
using System.Runtime.CompilerServices;

public struct Volume : IEquatable<Volume>,IFormattable
{
	public float value;
	public bool mute;

	private static readonly Volume zeroVolume = new Volume(0.0f,true);
	private static readonly Volume minVolume = new Volume(0.1f,true);
	private static readonly Volume maxVolume = new Volume(1.0f,true);

	public static Volume zero => zeroVolume;
	public static Volume min => minVolume;
	public static Volume max => maxVolume;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Volume(float value,bool mute)
	{
		this.value = value;
		this.mute = mute;
	}

	public void Set(float newVolume,bool newMute)
	{
		value = newVolume;
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

		return $"{value.ToString(format,formatProvider)} - Mute : {(mute ? "O" : "X")}";
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(value,mute);
	}

	public override bool Equals(object other)
	{
		if(!(other is Volume))
		{
			return false;
		}

		return Equals((Volume)other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Volume other)
	{
		return value == other.value && mute == other.mute;
	}

	public void Toggle()
	{
		mute = !mute;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Volume operator +(Volume left,Volume right)
	{
		return new Volume(left.value+right.value,left.mute || right.mute);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Volume operator -(Volume left,Volume right)
	{
		return new Volume(left.value-right.value,left.mute || right.mute);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Volume operator *(Volume left,Volume right)
	{
		return new Volume(left.value*right.value,left.mute || right.mute);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Volume operator /(Volume left,Volume right)
	{
		return new Volume(left.value/right.value,left.mute || right.mute);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Volume operator *(Volume left,float right)
	{
		return new Volume(left.value*right,left.mute);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Volume operator *(float left,Volume right)
	{
		return new Volume(right.value*left,right.mute);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Volume operator /(Volume left,float right)
	{
		return new Volume(left.value/right,left.mute);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Volume left,Volume right)
	{
		return left.Equals(right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Volume left,Volume right)
	{
		return !left.Equals(right);
	}
}