using System;
using MessagePack;
using MessagePack.Formatters;

namespace KZLib.KZData
{
	public static class DataProvider
	{
		public static IMessagePackFormatter[] FormatterArray => new IMessagePackFormatter[]
		{
			new GameVersionFormatter(), new SoundVolumeFormatter(),
		};

		#region GameVersion
		internal class GameVersionFormatter : IMessagePackFormatter<GameVersion?>
		{
			public void Serialize(ref MessagePackWriter writer,GameVersion? version,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(1);

				writer.Write(version?.ToString() ?? "");
			}

			public GameVersion Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 1)
				{
					throw new InvalidOperationException($"Reader is error. or {length} != 1");
				}

				return new GameVersion(reader.ReadString() ?? "");
			}
		}

		#endregion GameVersion

		#region SoundVolume
		internal class SoundVolumeFormatter : IMessagePackFormatter<SoundVolume>
		{
			public void Serialize(ref MessagePackWriter writer,SoundVolume volume,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(2);

				writer.Write(volume.level);
				writer.Write(volume.mute);
			}

			public SoundVolume Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 2)
				{
					throw new InvalidOperationException($"Reader is error. or {length} != 2");
				}

				return new SoundVolume(reader.ReadSingle(),reader.ReadBoolean());
			}
		}

		#endregion SoundVolume
	}
}