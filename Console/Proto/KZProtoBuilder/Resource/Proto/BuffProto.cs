#pragma warning disable CS8618
using MemoryPack;

namespace KZLib.Data
{
	[MemoryPackable]
	public partial class BuffProto : IBuffProto
	{
		[MemoryPackOrder(0)] public int Num { get; init; }

		[MemoryPackOrder(1)] public string BuffName { get; init; }
		[MemoryPackOrder(2)] public float Duration { get; init; }
		[MemoryPackOrder(3)] public int MaxStackCount { get; init; }
		[MemoryPackOrder(4)] public BuffEntry[] BuffEntryArray { get; }
	}
}
#pragma warning restore CS8618