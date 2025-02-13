using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace KZLib.KZData
{
	public struct ScreenResolution : IEquatable<ScreenResolution>,IFormattable
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

			return $"resolution : {width.ToString(format,formatProvider)}x{height.ToString(format,formatProvider)}, fullscreen : {fullscreen}";
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(ScreenResolution lhs,ScreenResolution rhs)
		{
			return lhs.Equals(rhs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(ScreenResolution lhs,ScreenResolution rhs)
		{
			return !lhs.Equals(rhs);
		}
	}
}