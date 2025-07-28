#pragma warning disable CS8618
using MessagePack;
using UnityEngine;

namespace KZLib.KZData
{
	[MessagePackObject]
	public partial class MotionProto : IProto
	{
		[Key(0)] public int Num { get; init; }

		[Key(1)] public string StateName { get; init; }
		[Key(2)] public MotionEvent[] EventArray { get; init; }
	}

	[MessagePackObject]
	public partial class MotionEvent
	{
		[Key(0)] public int Order { get; init; }
		[Key(1)] public string EffectPath { get; init; }
		[Key(2)] public Vector3 PositionOffset { get; init; }
		[Key(3)] public string StartBone { get; init; }
	}
}

#pragma warning restore CS8618 