#pragma warning disable CS8618
using MemoryPack;
using UnityEngine;

namespace KZLib.KZData
{
	[MemoryPackable]
	public partial class MotionProto : IProto
	{
		[MemoryPackOrder(0)] public int Num { get; init; }

		[MemoryPackOrder(1)] public string StateName { get; init; }
		[MemoryPackOrder(2)] public MotionEvent[] EventArray { get; init; }
	}

	[MemoryPackable]
	public partial class MotionEvent
	{
		[MemoryPackOrder(0)] public int Order { get; init; }
		[MemoryPackOrder(1)] public string EffectPath { get; init; }
		[MemoryPackOrder(2)] public Vector3 PositionOffset { get; init; }
		[MemoryPackOrder(3)] public string StartBone { get; init; }
	}
}
#pragma warning restore CS8618