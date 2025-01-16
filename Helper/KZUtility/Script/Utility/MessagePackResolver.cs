using UnityEngine;
using System;
using MessagePack;
using MessagePack.Formatters;
using KZLib.KZData;

namespace KZLib.KZUtility
{
	/// <summary>
	/// Supported Color, Color32, Vector2, Vector2Int, Vector3, Vector3Int, Vector4, Quaternion, Rect, RectInt, SoundVolume, ScreenResolution, Route
	/// <br/>
	/// <b> Example </b>
	/// <br/>
	/// var bytes = MessagePackSerializer.Serialize(object,MessagePackSerializerOptions.Standard.WithResolver(MessagePackResolver.Instance));
	/// <br/>
	/// <br/>
	/// var object = MessagePackSerializer.Deserialize(bytes,MessagePackSerializerOptions.Standard.WithResolver(MessagePackResolver.Instance));
	/// </summary>
	public class MessagePackResolver : IFormatterResolver
	{
		public static readonly MessagePackResolver Instance = new MessagePackResolver();

		public IMessagePackFormatter<T>? GetFormatter<T>()
		{
			var objectType = typeof(T);

			return objectType.Name switch
			{
				nameof(Color)				=> new ColorFormatter() as IMessagePackFormatter<T>,
				nameof(Color32)				=> new Color32Formatter() as IMessagePackFormatter<T>,

				nameof(Vector2)				=> new Vector2Formatter() as IMessagePackFormatter<T>,
				nameof(Vector3)				=> new Vector3Formatter() as IMessagePackFormatter<T>,
				nameof(Vector4)				=> new Vector4Formatter() as IMessagePackFormatter<T>,

				nameof(Vector2Int)			=> new Vector2IntFormatter() as IMessagePackFormatter<T>,
				nameof(Vector3Int)			=> new Vector2IntFormatter() as IMessagePackFormatter<T>,

				nameof(Quaternion)			=> new QuaternionFormatter() as IMessagePackFormatter<T>,

				nameof(Rect)				=> new RectFormatter() as IMessagePackFormatter<T>,
				nameof(RectInt)				=> new RectIntFormatter() as IMessagePackFormatter<T>,

				nameof(SoundVolume)			=> new SoundVolumeFormatter() as IMessagePackFormatter<T>,
				nameof(ScreenResolution)	=> new ScreenResolutionFormatter() as IMessagePackFormatter<T>,

				nameof(Route)				=> new RouteFormatter() as IMessagePackFormatter<T>,

				_ => throw new NotSupportedException($"NotSupported type {objectType.Name}"),
			};
		}

		#region Color
		internal class ColorFormatter : IMessagePackFormatter<Color>
		{
			public void Serialize(ref MessagePackWriter writer,Color color,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(color.r);
				writer.Write(color.g);
				writer.Write(color.b);
				writer.Write(color.a);
			}

			public Color Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"Reader is error. or {length} != 4");
				}

				return new Color(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Color

		#region Color32
		internal class Color32Formatter : IMessagePackFormatter<Color32>
		{
			public void Serialize(ref MessagePackWriter writer,Color32 color,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(color.r);
				writer.Write(color.g);
				writer.Write(color.b);
				writer.Write(color.a);
			}

			public Color32 Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 4");
				}

				return new Color32(reader.ReadByte(),reader.ReadByte(),reader.ReadByte(),reader.ReadByte());
			}
		}
		#endregion Color32

		#region Vector2
		internal class Vector2Formatter : IMessagePackFormatter<Vector2>
		{
			public void Serialize(ref MessagePackWriter writer,Vector2 vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(2);

				writer.Write(vector.x);
				writer.Write(vector.y);
			}

			public Vector2 Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 2)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 2");
				}

				return new Vector2(reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Vector2

		#region Vector2Int
		internal class Vector2IntFormatter : IMessagePackFormatter<Vector2Int>
		{
			public void Serialize(ref MessagePackWriter writer,Vector2Int vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(2);

				writer.Write(vector.x);
				writer.Write(vector.y);
			}

			public Vector2Int Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 2)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 2");
				}

				return new Vector2Int(reader.ReadInt32(),reader.ReadInt32());
			}
		}
		#endregion Vector2Int

		#region Vector3
		internal class Vector3Formatter : IMessagePackFormatter<Vector3>
		{
			public void Serialize(ref MessagePackWriter writer,Vector3 vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(3);

				writer.Write(vector.x);
				writer.Write(vector.y);
				writer.Write(vector.z);
			}

			public Vector3 Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 3)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 3");
				}

				return new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Vector3

		#region Vector3Int
		internal class Vector3IntFormatter : IMessagePackFormatter<Vector3Int>
		{
			public void Serialize(ref MessagePackWriter writer,Vector3Int vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(3);

				writer.Write(vector.x);
				writer.Write(vector.y);
				writer.Write(vector.z);
			}

			public Vector3Int Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 3)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 3");
				}

				return new Vector3Int(reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32());
			}
		}
		#endregion Vector3Int

		#region Vector4
		internal class Vector4Formatter : IMessagePackFormatter<Vector4>
		{
			public void Serialize(ref MessagePackWriter writer,Vector4 vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(vector.x);
				writer.Write(vector.y);
				writer.Write(vector.z);
				writer.Write(vector.w);
			}

			public Vector4 Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 4");
				}

				return new Vector4(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Vector4

		#region Quaternion
		internal class QuaternionFormatter : IMessagePackFormatter<Quaternion>
		{
			public void Serialize(ref MessagePackWriter writer,Quaternion quaternion,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(quaternion.x);
				writer.Write(quaternion.y);
				writer.Write(quaternion.z);
				writer.Write(quaternion.w);
			}

			public Quaternion Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 4");
				}

				return new Quaternion(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Quaternion

		#region Rect
		internal class RectFormatter : IMessagePackFormatter<Rect>
		{
			public void Serialize(ref MessagePackWriter writer,Rect rect,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(rect.x);
				writer.Write(rect.y);
				writer.Write(rect.width);
				writer.Write(rect.height);
			}

			public Rect Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 4");
				}

				return new Rect(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Rect

		#region RectInt
		internal class RectIntFormatter : IMessagePackFormatter<RectInt>
		{
			public void Serialize(ref MessagePackWriter writer,RectInt rect,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(rect.x);
				writer.Write(rect.y);
				writer.Write(rect.width);
				writer.Write(rect.height);
			}

			public RectInt Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 4");
				}

				return new RectInt(reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32());
			}
		}
		#endregion RectInt

		#region SoundVolume
		internal class SoundVolumeFormatter : IMessagePackFormatter<SoundVolume>
		{
			public void Serialize(ref MessagePackWriter writer,SoundVolume soundVolume,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(2);

				writer.Write(soundVolume.level);
				writer.Write(soundVolume.mute);
			}

			public SoundVolume Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 2)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 2");
				}

				return new SoundVolume(reader.ReadSingle(),reader.ReadBoolean());
			}
		}
		#endregion SoundVolume

		#region ScreenResolution
		internal class ScreenResolutionFormatter : IMessagePackFormatter<ScreenResolution>
		{
			public void Serialize(ref MessagePackWriter writer,ScreenResolution soundVolume,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(3);

				writer.Write(soundVolume.width);
				writer.Write(soundVolume.height);
				writer.Write(soundVolume.fullscreen);
			}

			public ScreenResolution Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 3)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 3");
				}

				return new ScreenResolution(reader.ReadInt32(),reader.ReadInt32(),reader.ReadBoolean());
			}
		}
		#endregion ScreenResolution

		#region Route
		internal class RouteFormatter : IMessagePackFormatter<Route>
		{
			public void Serialize(ref MessagePackWriter writer,Route soundVolume,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(1);

				writer.Write(soundVolume.AbsolutePath);
			}

			public Route Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 1)
				{
					throw new InvalidOperationException($"Reader error. or {length} != 3");
				}

				return new Route(reader.ReadString() ?? string.Empty);
			}
		}
		#endregion Route
	}
}