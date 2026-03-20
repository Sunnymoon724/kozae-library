#pragma warning disable CS8618
using MemoryPack;

namespace KZLib.Data
{
	[MemoryPackable]
	public partial class ColorProto : IColorProto
	{
		[MemoryPackOrder(0)] public int Num { get; init; }
		[MemoryPackOrder(1)] public string[] ColorArray { get; init; }
	}
}
#pragma warning restore CS8618