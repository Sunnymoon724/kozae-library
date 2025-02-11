using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace KZLib.KZData
{
	public struct ScreenResolution : IEquatable<ScreenResolution>,IFormattable,IComparable,IComparable<ScreenResolution>
	{
		public int width;
		public int height;
		public bool fullscreen;

		private static readonly ScreenResolution sdScreenResolution = new ScreenResolution(720,480,true);
		private static readonly ScreenResolution hdScreenResolution = new ScreenResolution(1280,720,true);
		private static readonly ScreenResolution fhdScreenResolution = new ScreenResolution(1920,1080,true);
		private static readonly ScreenResolution qhdScreenResolution = new ScreenResolution(2560,1440,true);
		private static readonly ScreenResolution uhdScreenResolution = new ScreenResolution(3840,2160,true);

		public static ScreenResolution sd => sdScreenResolution;
		public static ScreenResolution hd => hdScreenResolution;
		public static ScreenResolution fhd => fhdScreenResolution;
		public static ScreenResolution qhd => qhdScreenResolution;
		public static ScreenResolution uhd => uhdScreenResolution;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ScreenResolution(int width,int height,bool fullscreen)
		{
			this.width = width;
			this.height = height;
			this.fullscreen = fullscreen;
		}

		public void Set(int newWidth,int newHeight,bool newFullscreen)
		{
			width = newWidth;
			height = newHeight;
			fullscreen = newFullscreen;
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
			formatProvider ??= CultureInfo.InvariantCulture;

			return $"{width.ToString(format,formatProvider)}x{height.ToString(format,formatProvider)} - Fullscreen : {(fullscreen ? "O" : "X")}";
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(width,height,fullscreen);
		}

		public override bool Equals(object other)
		{
			return other is ScreenResolution resolution && Equals(resolution);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ScreenResolution other)
		{
			return width == other.width && height == other.height && fullscreen == other.fullscreen;
		}

		public int CompareTo(object other)
		{
			if(other is ScreenResolution resolution)
			{
				return CompareTo(resolution);
			}

			throw new ArgumentException($"{other} is not a ScreenResolution");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(ScreenResolution other)
		{
			var currentResolution = width*height;
			var otherResolution = other.width*other.height;

			return currentResolution.CompareTo(otherResolution);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(ScreenResolution left,ScreenResolution right)
		{
			return left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(ScreenResolution left,ScreenResolution right)
		{
			return !left.Equals(right);
		}

		public static ScreenResolution Parse(string value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static ScreenResolution Parse(string value,IFormatProvider provider)
		{
			return Parse(value.AsSpan(),provider);
		}

		public static ScreenResolution Parse(ReadOnlySpan<char> value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static ScreenResolution Parse(ReadOnlySpan<char> value,IFormatProvider provider)
		{
			//? $"AAAxBBB - Fullscreen : C";

			var index1 = value.IndexOf('x');

			if(index1 == -1)
			{
				throw new FormatException("Invalid format for ScreenResolution.");
			}

			var widthPart = value[..index1];

			if(!int.TryParse(widthPart,NumberStyles.Integer,provider,out var width))
			{
				throw new FormatException($"Invalid width format in {widthPart.ToString()}");
			}

			var index2 = value.IndexOf('-');

			if(index2 == -1)
			{
				throw new FormatException("Invalid format for ScreenResolution.");
			}

			var heightPart = value[(index1+1)..index2];

			if(!int.TryParse(heightPart,NumberStyles.Integer,provider,out var height))
			{
				throw new FormatException($"Invalid height format in {heightPart.ToString()}");
			}

			var index3 = value.IndexOf(':');

			if(index3 == -1)
			{
				throw new FormatException("Invalid format for ScreenResolution.");
			}

			var fullscreenPart = value[(index3+1)..].Trim();

			if(fullscreenPart != "O" && fullscreenPart != "X")
			{
				throw new FormatException($"Invalid fullscreen format in {fullscreenPart.ToString()}");
			}

			var fullscreen = fullscreenPart == "O";

			return new ScreenResolution(width,height,fullscreen);
		}

		public static bool TryParse(string value,out ScreenResolution resolution)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out resolution);
		}

		public static bool TryParse(string value,IFormatProvider provider,out ScreenResolution resolution)
		{
			return TryParse(value.AsSpan(),provider,out resolution);
		}

		public static bool TryParse(ReadOnlySpan<char> value,out ScreenResolution resolution)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out resolution);
		}

		public static bool TryParse(ReadOnlySpan<char> value,IFormatProvider provider,out ScreenResolution resolution)
		{
			try
			{
				resolution = Parse(value,provider);

				return true;
			}
			catch
			{
				resolution = default;

				return false;
			}
		}
	}
}