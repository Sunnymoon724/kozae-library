#pragma warning disable CS8618
using MessagePack;

namespace KZLib.KZData
{
	[MessagePackObject]
	public partial class NetworkErrorProto : IProto
	{
		[Key(0)] public int Num { get; init; }

		[Key(1)] public string Description { get; init; }
		[Key(2)] public NetworkErrorResultType ResultMainType { get; init; }
		[Key(3)] public NetworkErrorResultType ResultSubType { get; init; }
	}
}

#pragma warning restore CS8618 