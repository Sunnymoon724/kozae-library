using UnityEngine;
using System;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System.Collections.Generic;

namespace KZLib.KZResolver
{
	/// <summary>
	/// <br/> support Color, Color32, Vector2, Vector2Int, Vector3, Vector3Int, Vector4, Quaternion, Rect, RectInt
	/// </summary>
	public static class MessagePackProvider
	{
		public static void SetDefaultSettings()
		{
			var formatterList = new List<IMessagePackFormatter>
			{
				new ColorFormatter()
			};

			formatterList.AddRange(DataProvider.FormatterArray);

			MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(formatterList,
			new[] { StandardResolver.Instance }));
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
					throw new InvalidOperationException($"reader error. or {length} != 4");
				}

				return new Color(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Color

		#region Color32
		internal class Color32Formatter : IMessagePackFormatter<Color32>
		{
			public void Serialize(ref MessagePackWriter writer,Color32 _color,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(_color.r);
				writer.Write(_color.g);
				writer.Write(_color.b);
				writer.Write(_color.a);
			}

			public Color32 Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"reader error. or {length} != 4");
				}

				return new Color32(reader.ReadByte(),reader.ReadByte(),reader.ReadByte(),reader.ReadByte());
			}
		}
		#endregion Color32

		#region Vector2
		internal class Vector2Formatter : IMessagePackFormatter<Vector2>
		{
			public void Serialize(ref MessagePackWriter writer,Vector2 _vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(2);

				writer.Write(_vector.x);
				writer.Write(_vector.y);
			}

			public Vector2 Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 2)
				{
					throw new InvalidOperationException($"reader error. or {length} != 2");
				}

				return new Vector2(reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Vector2

		#region Vector2Int
		internal class Vector2IntFormatter : IMessagePackFormatter<Vector2Int>
		{
			public void Serialize(ref MessagePackWriter writer,Vector2Int _vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(2);

				writer.Write(_vector.x);
				writer.Write(_vector.y);
			}

			public Vector2Int Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 2)
				{
					throw new InvalidOperationException($"reader error. or {length} != 2");
				}

				return new Vector2Int(reader.ReadInt32(),reader.ReadInt32());
			}
		}
		#endregion Vector2Int

		#region Vector3
		internal class Vector3Formatter : IMessagePackFormatter<Vector3>
		{
			public void Serialize(ref MessagePackWriter writer,Vector3 _vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(3);

				writer.Write(_vector.x);
				writer.Write(_vector.y);
				writer.Write(_vector.z);
			}

			public Vector3 Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 3)
				{
					throw new InvalidOperationException($"reader error. or {length} != 3");
				}

				return new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Vector3

		#region Vector3Int
		internal class Vector3IntFormatter : IMessagePackFormatter<Vector3Int>
		{
			public void Serialize(ref MessagePackWriter writer,Vector3Int _vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(3);

				writer.Write(_vector.x);
				writer.Write(_vector.y);
				writer.Write(_vector.z);
			}

			public Vector3Int Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 3)
				{
					throw new InvalidOperationException($"reader error. or {length} != 3");
				}

				return new Vector3Int(reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32());
			}
		}
		#endregion Vector3Int

		#region Vector4
		internal class Vector4Formatter : IMessagePackFormatter<Vector4>
		{
			public void Serialize(ref MessagePackWriter writer,Vector4 _vector,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(_vector.x);
				writer.Write(_vector.y);
				writer.Write(_vector.z);
				writer.Write(_vector.w);
			}

			public Vector4 Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"reader error. or {length} != 4");
				}

				return new Vector4(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Vector4

		#region Quaternion
		internal class QuaternionFormatter : IMessagePackFormatter<Quaternion>
		{
			public void Serialize(ref MessagePackWriter writer,Quaternion _quaternion,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(_quaternion.x);
				writer.Write(_quaternion.y);
				writer.Write(_quaternion.z);
				writer.Write(_quaternion.w);
			}

			public Quaternion Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"reader error. or {length} != 4");
				}

				return new Quaternion(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Quaternion

		#region Rect
		internal class RectFormatter : IMessagePackFormatter<Rect>
		{
			public void Serialize(ref MessagePackWriter writer,Rect _rect,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(_rect.x);
				writer.Write(_rect.y);
				writer.Write(_rect.width);
				writer.Write(_rect.height);
			}

			public Rect Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"reader error. or {length} != 4");
				}

				return new Rect(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
			}
		}
		#endregion Rect

		#region RectInt
		internal class RectIntFormatter : IMessagePackFormatter<RectInt>
		{
			public void Serialize(ref MessagePackWriter writer,RectInt _rect,MessagePackSerializerOptions options)
			{
				writer.WriteArrayHeader(4);

				writer.Write(_rect.x);
				writer.Write(_rect.y);
				writer.Write(_rect.width);
				writer.Write(_rect.height);
			}

			public RectInt Deserialize(ref MessagePackReader reader,MessagePackSerializerOptions options)
			{
				if(!reader.TryReadArrayHeader(out int length) || length != 4)
				{
					throw new InvalidOperationException($"reader error. or {length} != 4");
				}

				return new RectInt(reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32());
			}
		}
		#endregion RectInt
	}
}