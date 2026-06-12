using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using MemoryPack;

namespace KZLib.Data
{
	/// <summary>
	/// Value type representing screen resolution and fullscreen mode.
	/// </summary>
	[MemoryPackable]
	public partial struct ScreenResolution : IEquatable<ScreenResolution>,IFormattable
	{
		/// <summary>Width in pixels.</summary>
		public int width;

		/// <summary>Height in pixels.</summary>
		public int height;

		/// <summary>Whether fullscreen mode is enabled.</summary>
		public bool fullscreen;

		private static readonly ScreenResolution sdScreenResolution		= new(720,480,true);
		private static readonly ScreenResolution hdScreenResolution		= new(1280,720,true);
		private static readonly ScreenResolution fhdScreenResolution	= new(1920,1080,true);
		private static readonly ScreenResolution qhdScreenResolution	= new(2560,1440,true);
		private static readonly ScreenResolution uhdScreenResolution	= new(3840,2160,true);

		/// <summary>SD (720x480) fullscreen preset.</summary>
		public static ScreenResolution sd	=> sdScreenResolution;

		/// <summary>HD (1280x720) fullscreen preset.</summary>
		public static ScreenResolution hd	=> hdScreenResolution;

		/// <summary>Full HD (1920x1080) fullscreen preset.</summary>
		public static ScreenResolution fhd	=> fhdScreenResolution;

		/// <summary>QHD (2560x1440) fullscreen preset.</summary>
		public static ScreenResolution qhd	=> qhdScreenResolution;

		/// <summary>UHD (3840x2160) fullscreen preset.</summary>
		public static ScreenResolution uhd	=> uhdScreenResolution;

		/// <summary>
		/// Creates an instance with the given resolution and fullscreen flag.
		/// width and height must be greater than zero.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ScreenResolution(int width,int height,bool fullscreen)
		{
			_ValidateDimension(width,nameof(width));
			_ValidateDimension(height,nameof(height));

			this.width = width;
			this.height = height;
			this.fullscreen = fullscreen;
		}

		/// <summary>Updates resolution and fullscreen flag.</summary>
		public void Set(int newWidth,int newHeight,bool newFullscreen)
		{
			_ValidateDimension(newWidth,nameof(newWidth));
			_ValidateDimension(newHeight,nameof(newHeight));

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

		/// <summary>
		/// Returns a string in the format <c>resolution : {width}x{height}, fullscreen : {fullscreen}</c>.
		/// </summary>
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
			if(!_TryParseCore(value,provider,out var resolution))
			{
				throw new FormatException($"Invalid ScreenResolution format in '{value.ToString()}'");
			}

			return resolution;
		}

		public static ScreenResolution Parse(string value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Parses a string produced by <see cref="ToString()"/> into a <see cref="ScreenResolution"/>.
		/// </summary>
		public static ScreenResolution Parse(string value,IFormatProvider provider)
		{
			if(!_TryParseCore(value.AsSpan(),provider,out var resolution))
			{
				throw new FormatException($"Invalid ScreenResolution format in '{value}'");
			}

			return resolution;
		}

		public static bool TryParse(ReadOnlySpan<char> value,out ScreenResolution resolution)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out resolution);
		}

		public static bool TryParse(ReadOnlySpan<char> value,IFormatProvider provider,out ScreenResolution resolution)
		{
			return _TryParseCore(value,provider,out resolution);
		}

		public static bool TryParse(string value,out ScreenResolution resolution)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out resolution);
		}

		public static bool TryParse(string value,IFormatProvider provider,out ScreenResolution resolution)
		{
			return _TryParseCore(value.AsSpan(),provider,out resolution);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void _ValidateDimension(int dimension,string paramName)
		{
			if(dimension <= 0)
			{
				throw new ArgumentOutOfRangeException(paramName,dimension,"Resolution dimensions must be greater than zero.");
			}
		}

		private static bool _TryParseCore(ReadOnlySpan<char> value,IFormatProvider provider,out ScreenResolution resolution)
		{
			resolution = default;

			var span = value.Trim();

			if(span.IsEmpty)
			{
				return false;
			}

			if(!_TryFindToken(ref span,"resolution"))
			{
				return false;
			}

			if(!_TryConsumeSeparator(ref span))
			{
				return false;
			}

			var xIndex = span.IndexOf('x');

			if(xIndex <= 0)
			{
				return false;
			}

			var widthSpan = span[..xIndex].Trim();

			span = span[(xIndex+1)..];

			var commaIndex = span.IndexOf(',');

			if(commaIndex <= 0)
			{
				return false;
			}

			var heightSpan = span[..commaIndex].Trim();
			span = span[(commaIndex+1)..].TrimStart();

			if(!int.TryParse(widthSpan,NumberStyles.Integer,provider,out var width) || !int.TryParse(heightSpan,NumberStyles.Integer,provider,out var height) || width <= 0 || height <= 0)
			{
				return false;
			}

			if(!_TryFindToken(ref span,"fullscreen"))
			{
				return false;
			}

			if(!_TryConsumeSeparator(ref span))
			{
				return false;
			}

			if(!_TryParseBool(span.Trim(),out var fullscreen))
			{
				return false;
			}

			resolution = new ScreenResolution(width,height,fullscreen);

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
