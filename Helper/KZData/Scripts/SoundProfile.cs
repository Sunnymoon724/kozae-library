using MemoryPack;

namespace KZLib.KZData
{
	[MemoryPackable]
	public partial record SoundProfile
	{
		public SoundVolume Master { get; init; }
		public SoundVolume Music { get; init; }
		public SoundVolume Effect { get; init; }

		public SoundProfile(SoundVolume master,SoundVolume music,SoundVolume effect)
		{
			Master = master; 
			Music = music;
			Effect = effect;
		}

		public SoundVolume OutputMusic => _CalculateOutputVolume(Music);
		public SoundVolume OutputEffect => _CalculateOutputVolume(Effect);

		private SoundVolume _CalculateOutputVolume(SoundVolume primary)
		{
			float outputLevel = Master.level * primary.level;
			bool outputMute = Master.mute || primary.mute;

			return new SoundVolume(outputLevel, outputMute);
		}
	}
}