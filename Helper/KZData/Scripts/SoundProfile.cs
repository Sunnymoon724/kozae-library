using System;
using System.Globalization;
using MemoryPack;

namespace KZLib.Data
{
	/// <summary>
	/// Sound profile grouping master, music, and effect channel volumes.
	/// <see cref="outputMusic"/> and <see cref="outputEffect"/> multiply master and channel levels for final output.
	/// </summary>
	[MemoryPackable]
	public partial record SoundProfile
	{
		private static readonly SoundProfile s_maxSoundProfile = new(SoundVolume.max,SoundVolume.max,SoundVolume.max);

		/// <summary>Default profile with all channels at maximum volume.</summary>
		public static SoundProfile maxSoundProfile => s_maxSoundProfile;

		/// <summary>Master channel volume (global scale).</summary>
		public SoundVolume master { get; init; }

		/// <summary>Background music channel volume.</summary>
		public SoundVolume music { get; init; }

		/// <summary>Sound effect channel volume.</summary>
		public SoundVolume effect { get; init; }

		/// <summary>Creates a profile from master, music, and effect channel volumes.</summary>
		public SoundProfile(SoundVolume master,SoundVolume music,SoundVolume effect)
		{
			this.master = master;
			this.music = music;
			this.effect = effect;
		}

		/// <summary>Final music output volume (master x music).</summary>
		public SoundVolume outputMusic => _CalculateOutputVolume(music);

		/// <summary>Final effect output volume (master x effect).</summary>
		public SoundVolume outputEffect => _CalculateOutputVolume(effect);

		/// <summary>Returns a copy with an updated master channel.</summary>
		public SoundProfile WithMaster(SoundVolume master) => this with { master = master };

		/// <summary>Returns a copy with an updated music channel.</summary>
		public SoundProfile WithMusic(SoundVolume music) => this with { music = music };

		/// <summary>Returns a copy with an updated effect channel.</summary>
		public SoundProfile WithEffect(SoundVolume effect) => this with { effect = effect };

		/// <summary>
		/// Returns a string in the format <c>master : ..., music : ..., effect : ...</c>.
		/// </summary>
		public override string ToString()
		{
			return $"master : {master}, music : {music}, effect : {effect}";
		}

		public static SoundProfile Parse(ReadOnlySpan<char> value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		public static SoundProfile Parse(ReadOnlySpan<char> value,IFormatProvider provider)
		{
			if(!_TryParseCore(value,provider,out var profile))
			{
				throw new FormatException($"Invalid SoundProfile format in '{value.ToString()}'");
			}

			return profile;
		}

		public static SoundProfile Parse(string value)
		{
			return Parse(value,CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Parses a string produced by <see cref="ToString()"/> into a <see cref="SoundProfile"/>.
		/// </summary>
		public static SoundProfile Parse(string value,IFormatProvider provider)
		{
			if(!_TryParseCore(value.AsSpan(),provider,out var profile))
			{
				throw new FormatException($"Invalid SoundProfile format in '{value}'");
			}

			return profile;
		}

		public static bool TryParse(ReadOnlySpan<char> value,out SoundProfile profile)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out profile);
		}

		public static bool TryParse(ReadOnlySpan<char> value,IFormatProvider provider,out SoundProfile profile)
		{
			return _TryParseCore(value,provider,out profile);
		}

		public static bool TryParse(string value,out SoundProfile profile)
		{
			return TryParse(value,CultureInfo.InvariantCulture,out profile);
		}

		public static bool TryParse(string value,IFormatProvider provider,out SoundProfile profile)
		{
			return _TryParseCore(value.AsSpan(),provider,out profile);
		}

		/// <summary>
		/// Multiplies master and channel levels; mute is combined with logical OR.
		/// </summary>
		private SoundVolume _CalculateOutputVolume(SoundVolume volume)
		{
			var level = master.level*volume.level;
			var mute = master.mute || volume.mute;

			return new SoundVolume(level,mute);
		}

		private static bool _TryParseCore(ReadOnlySpan<char> value,IFormatProvider provider,out SoundProfile profile)
		{
			profile = maxSoundProfile;

			var span = value.Trim();

			if(span.IsEmpty)
			{
				return false;
			}

			if(!_TryExtractChannel(ref span,"master","music",out var masterSpan) || !SoundVolume.TryParse(masterSpan,provider,out var master))
			{
				return false;
			}

			if(!_TryExtractChannel(ref span,"music","effect",out var musicSpan) || !SoundVolume.TryParse(musicSpan,provider,out var music))
			{
				return false;
			}

			if(!_TryExtractChannel(ref span,"effect",ReadOnlySpan<char>.Empty,out var effectSpan) || !SoundVolume.TryParse(effectSpan,provider,out var effect))
			{
				return false;
			}

			profile = new SoundProfile(master,music,effect);

			return true;
		}

		private static bool _TryExtractChannel(ref ReadOnlySpan<char> span,ReadOnlySpan<char> channelName,ReadOnlySpan<char> nextChannelName,out ReadOnlySpan<char> channelValue)
		{
			channelValue = default;

			if(!_TryFindToken(ref span,channelName))
			{
				return false;
			}

			if(!_TryConsumeSeparator(ref span))
			{
				return false;
			}

			if(!nextChannelName.IsEmpty)
			{
				var nextIndex = span.IndexOf(nextChannelName,StringComparison.OrdinalIgnoreCase);

				if(nextIndex < 0)
				{
					return false;
				}

				var endIndex = nextIndex;

				while(endIndex > 0 && (span[endIndex-1] == ',' || char.IsWhiteSpace(span[endIndex-1])))
				{
					endIndex--;
				}

				channelValue = span[..endIndex].Trim();

				span = span[nextIndex..];

				return !channelValue.IsEmpty;
			}

			channelValue = span.Trim();

			span = ReadOnlySpan<char>.Empty;

			return !channelValue.IsEmpty;
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
	}
}