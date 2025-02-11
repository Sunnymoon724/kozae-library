#pragma warning disable CS8618
using MessagePack;
using UnityEngine;

namespace KZLib.KZData
{
	[MessagePackObject]
	public class MotionProto : IProto
	{
		[Key(0)] public int Num { get; set; }

		[Key(1)] public string StateName { get; set; }
		[Key(2)] public MotionEvent[] EventArray { get; set; }
	}

	[MessagePackObject]
	public class MotionEvent
	{
		[Key(0)] public int Order { get; set; }
		[Key(1)] public string EffectPath { get; set; }
		[Key(2)] public Vector3 PositionOffset { get; set; }
		[Key(3)] public string StartBone { get; set; }
	}
}

#pragma warning restore CS8618 