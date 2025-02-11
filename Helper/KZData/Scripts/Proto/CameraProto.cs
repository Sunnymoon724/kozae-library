using MessagePack;
using UnityEngine;

namespace KZLib.KZData
{
	[MessagePackObject]
	public class CameraProto : IProto
	{
		[Key(0)]
		public int Num { get; set; }

		[Key(1)]
		public bool Orthographic { get; set; }

		[Key(2)]
		public float NearClipPlane { get; set; }
		[Key(3)]
		public float FarClipPlane { get; set; }

		[Key(4)]
		public float FieldOfView { get; set; }

		[Key(5)]
		public Vector3 Position { get; set; }
		[Key(6)]
		public Vector3 Rotation { get; set; }
	}
}