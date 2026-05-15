#pragma warning disable CS8618
using MemoryPack;
using UnityEngine;

namespace KZLib.Data
{
	[MemoryPackable]
	public partial class MotionEntry
	{
		[MemoryPackOrder(0)] public int Order { get; init; }
		[MemoryPackOrder(1)] public string EffectPath { get; init; }
		[MemoryPackOrder(2)] public Vector3 PositionOffset { get; init; }
		[MemoryPackOrder(3)] public string StartBone { get; init; }
	}

}
#pragma warning restore CS8618