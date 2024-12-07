using System;
using MessagePack;
using MessagePack.Formatters;

namespace KZLib.KZResolver
{
	public static class DataProvider
	{
		public static IMessagePackFormatter[] FormatterArray => new IMessagePackFormatter[]
		{
			new VersionFormatter(),
		};

		#region Version
		internal class VersionFormatter : IMessagePackFormatter<Version?>
		{
			public void Serialize(ref MessagePackWriter writer,Version? version,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(1);
				writer.Write(version?.ToString() ?? "");
			}

			public Version Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 1)
				{
					throw new InvalidOperationException($"Invalid array header. Expected length 1, but got {length}.");
				}

				return new Version(reader.ReadString() ?? "");
			}
		}

		#endregion Version
	}
}