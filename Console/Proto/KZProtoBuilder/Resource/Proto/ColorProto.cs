#pragma warning disable CS8618
using MemoryPack;

namespace KZLib.Data
{
	[MemoryPackable]
	public partial class ColorProto : IProto
	{
		[MemoryPackOrder(0)] public int Num { get; init; }

		[MemoryPackOrder(1)] public string NameKey { get; init; }
		[MemoryPackOrder(2)] public string[] ColorArray { get; init; }
	}
}
#pragma warning restore CS8618