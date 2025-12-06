#pragma warning disable CS8618
using MemoryPack;

namespace KZLib.KZData
{
	[MemoryPackable]
	public partial class NetworkErrorProto : IProto
	{
		[MemoryPackOrder(0)] public int Num { get; init; }

		[MemoryPackOrder(1)] public string Description { get; init; }
		[MemoryPackOrder(2)] public NetworkErrorResultType ResultMainType { get; init; }
		[MemoryPackOrder(3)] public NetworkErrorResultType ResultSubType { get; init; }
	}
}
#pragma warning restore CS8618 