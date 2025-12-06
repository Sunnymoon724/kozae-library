using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MemoryPack;

namespace KZLib.KZData
{
	[MemoryPackable]
	public partial struct ScreenResolution : IEquatable<ScreenResolution>,IFormattable
	{
		public int width;
		public int height;
		public bool fullscreen;

		private static readonly ScreenResolution sdScreenResolution		= new(720,480,true);
		private static readonly ScreenResolution hdScreenResolution		= new(1280,720,true);
		private static readonly ScreenResolution fhdScreenResolution	= new(1920,1080,true);
		private static readonly ScreenResolution qhdScreenResolution	= new(2560,1440,true);
		private static readonly ScreenResolution uhdScreenResolution	= new(3840,2160,true);

		public static ScreenResolution sd	=> sdScreenResolution;
		public static ScreenResolution hd	=> hdScreenResolution;
		public static ScreenResolution fhd	=> fhdScreenResolution;
		public static ScreenResolution qhd	=> qhdScreenResolution;
		public static ScreenResolution uhd	=> uhdScreenResolution;

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

		public string ToString(string? format,IFormatProvider? formatProvider)
		{
			formatProvider ??= CultureInfo.InvariantCulture;

			return $"resolution : {width.ToString(format,formatProvider)}x{height.ToString(format,formatProvider)}, fullscreen : {fullscreen}";
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(width,height,fullscreen);
		}

		public override bool Equals(object? other)
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

		public static ScreenResolution Parse(ReadOnlySpan<char> value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static ScreenResolution Parse(ReadOnlySpan<char> value,IFormatProvider provider)
		{
			return Parse(value.ToString(),provider);
		}

		public static ScreenResolution Parse(string value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static ScreenResolution Parse(string value,IFormatProvider provider)
		{
			var resolutionRegex = new Regex(@"resolution\s*:\s*(\d+)x(\d+)");
			var resolutionMatch = resolutionRegex.Match(value);

			if(!resolutionMatch.Success || !int.TryParse(resolutionMatch.Groups[1].Value,NumberStyles.Integer,provider,out var width) || !int.TryParse(resolutionMatch.Groups[2].Value,NumberStyles.Integer,provider,out var height))
			{
				throw new FormatException($"Invalid resolution format in {resolutionMatch}");
			}

			var fullscreenRegex = new Regex(@"fullscreen\s*:\s*(true|false)",RegexOptions.IgnoreCase);
			var fullscreenMatch = fullscreenRegex.Match(value);

			if(!fullscreenMatch.Success || !bool.TryParse(fullscreenMatch.Groups[1].Value,out var fullscreen))
			{
				throw new FormatException($"Invalid fullscreen format in {fullscreenMatch}");
			}

			return new ScreenResolution(width,height,fullscreen);
		}

		public static bool TryParse(ReadOnlySpan<char> value,out ScreenResolution resolution)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out resolution);
		}

		public static bool TryParse(ReadOnlySpan<char> value,IFormatProvider provider,out ScreenResolution resolution)
		{
			return TryParse(value.ToString(),provider,out resolution);
		}

		public static bool TryParse(string value,out ScreenResolution resolution)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out resolution);
		}

		public static bool TryParse(string value,IFormatProvider provider,out ScreenResolution resolution)
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