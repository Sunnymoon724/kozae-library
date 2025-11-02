#pragma warning disable CS8618
using MessagePack;

namespace KZLib.KZData
{
	[MessagePackObject]
	public partial class ColorProto : IProto
	{
		[Key(0)] public int Num { get; init; }

		[Key(1)] public string NameKey { get; init; }
		[Key(2)] public string[] ColorArray { get; init; }
	}
}
#pragma warning restore CS8618