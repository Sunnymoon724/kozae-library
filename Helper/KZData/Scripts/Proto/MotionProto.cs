#pragma warning disable CS8618
using MessagePack;
using UnityEngine;

namespace KZLib.KZData
{
	[MessagePackObject]
	public partial class MotionProto : IProto
	{
		[Key(0)] public int Num { get; private set; }

		[Key(1)] public string StateName { get; private set; }
		[Key(2)] public MotionEvent[] EventArray { get; private set; }
	}

	[MessagePackObject]
	public partial class MotionEvent
	{
		[Key(0)] public int Order { get; private set; }
		[Key(1)] public string EffectPath { get; private set; }
		[Key(2)] public Vector3 PositionOffset { get; private set; }
		[Key(3)] public string StartBone { get; private set; }
	}
}

#pragma warning restore CS8618 