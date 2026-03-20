#pragma warning disable CS8618
using MemoryPack;

namespace KZLib.Data
{
	[MemoryPackable]
	public partial class MotionProto : IMotionProto
	{
		[MemoryPackOrder(0)] public int Num { get; init; }

		[MemoryPackOrder(1)] public string StateName { get; init; }
		[MemoryPackOrder(2)] public MotionEvent[] EventArray { get; init; }
	}
}
#pragma warning restore CS8618