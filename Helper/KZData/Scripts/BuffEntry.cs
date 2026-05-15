#pragma warning disable CS8618
using MemoryPack;

namespace KZLib.Data
{
	[MemoryPackable]
	public partial class BuffEntry
	{
		[MemoryPackOrder(0)] public string Id { get; init; }
		[MemoryPackOrder(1)] public string StatName { get; init; }
		[MemoryPackOrder(2)] public float Value { get; init; }
		[MemoryPackOrder(3)] public bool IsPercent { get; init; }
	}

}
#pragma warning restore CS8618