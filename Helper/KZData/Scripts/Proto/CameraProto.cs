using MessagePack;
using UnityEngine;

namespace KZLib.KZData
{
	[MessagePackObject]
	public partial class CameraProto : IProto
	{
		[Key(0)]
		public int Num { get; private set; }

		[Key(1)]
		public bool Orthographic { get; private set; }

		[Key(2)]
		public float NearClipPlane { get; private set; }
		[Key(3)]
		public float FarClipPlane { get; private set; }

		[Key(4)]
		public float FieldOfView { get; private set; }

		[Key(5)]
		public Vector3 Position { get; private set; }
		[Key(6)]
		public Vector3 Rotation { get; private set; }
	}
}