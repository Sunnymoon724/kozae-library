using UnityEngine;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KZLib.KZUtility
{
	/// <summary>
	/// <br/> support Color, Color32, Vector2, Vector2Int, Vector3, Vector3Int, Vector4, Quaternion, Rect, RectInt
	/// </summary>
	public static class JsonProvider
	{
		public static void SetConverterSettings()
		{
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				Converters = new JsonConverter[]
				{
					new ColorJsonConverter(),	new Color32JsonConverter(), 
					new Vector2JsonConverter(), new Vector2IntJsonConverter(), 
					new Vector3JsonConverter(), new Vector3IntJsonConverter(), 
					new Vector4JsonConverter(), new QuaternionJsonConverter(), 
					new RectJsonConverter(),	new RectIntJsonConverter(),
				}
			};
		}

		#region Color
		private class ColorJsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Color);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var r 	= jsonObject["r"];
				var g	= jsonObject["g"];
				var b	= jsonObject["b"];
				var a	= jsonObject["a"];

				if(r == null || g == null || b == null || a == null)
				{
					return null;
				}

				return new Color(r.Value<float>(),g.Value<float>(),b.Value<float>(),a.Value<float>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Color color))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("r");	writer.WriteValue(color.r);
				writer.WritePropertyName("g");	writer.WriteValue(color.g);
				writer.WritePropertyName("b");	writer.WriteValue(color.b);
				writer.WritePropertyName("a");	writer.WriteValue(color.a);

				writer.WriteEndObject();
			}
		}
		#endregion Color

		#region Color32
		private class Color32JsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Color32);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var r 	= jsonObject["r"];
				var g	= jsonObject["g"];
				var b	= jsonObject["b"];
				var a	= jsonObject["a"];

				if(r == null || g == null || b == null || a == null)
				{
					return null;
				}

				return new Color32(r.Value<byte>(),g.Value<byte>(),b.Value<byte>(),a.Value<byte>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Color32 color))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("r");	writer.WriteValue(color.r);
				writer.WritePropertyName("g");	writer.WriteValue(color.g);
				writer.WritePropertyName("b");	writer.WriteValue(color.b);
				writer.WritePropertyName("a");	writer.WriteValue(color.a);

				writer.WriteEndObject();
			}
		}
		#endregion Color32

		#region Vector2
		private class Vector2JsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Vector2);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var x 	= jsonObject["x"];
				var y	= jsonObject["y"];

				if(x == null || y == null)
				{
					return null;
				}

				return new Vector2(x.Value<float>(),y.Value<float>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Vector2 vector2))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("x");	writer.WriteValue(vector2.x);
				writer.WritePropertyName("y");	writer.WriteValue(vector2.y);

				writer.WriteEndObject();
			}
		}
		#endregion Vector2

		#region Vector2Int
		private class Vector2IntJsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Vector2Int);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var x 	= jsonObject["x"];
				var y	= jsonObject["y"];

				if(x == null || y == null)
				{
					return null;
				}

				return new Vector2Int(x.Value<int>(),y.Value<int>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Vector2Int vector2))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("x");	writer.WriteValue(vector2.x);
				writer.WritePropertyName("y");	writer.WriteValue(vector2.y);

				writer.WriteEndObject();
			}
		}
		#endregion Vector2Int

		#region Vector3
		private class Vector3JsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Vector3);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var x 	= jsonObject["x"];
				var y	= jsonObject["y"];
				var z	= jsonObject["z"];

				if(x == null || y == null || z == null)
				{
					return null;
				}

				return new Vector3(x.Value<float>(),y.Value<float>(),z.Value<float>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Vector3 vector3))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("x");	writer.WriteValue(vector3.x);
				writer.WritePropertyName("y");	writer.WriteValue(vector3.y);
				writer.WritePropertyName("z");	writer.WriteValue(vector3.z);

				writer.WriteEndObject();
			}
		}
		#endregion Vector3

		#region Vector3Int
		private class Vector3IntJsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Vector3Int);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var x 	= jsonObject["x"];
				var y	= jsonObject["y"];
				var z	= jsonObject["z"];

				if(x == null || y == null || z == null)
				{
					return null;
				}

				return new Vector3Int(x.Value<int>(),y.Value<int>(),z.Value<int>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Vector3Int vector3))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("x");	writer.WriteValue(vector3.x);
				writer.WritePropertyName("y");	writer.WriteValue(vector3.y);
				writer.WritePropertyName("z");	writer.WriteValue(vector3.z);

				writer.WriteEndObject();
			}
		}
		#endregion Vector3Int

		#region Vector4
		private class Vector4JsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Vector4);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var x 	= jsonObject["x"];
				var y	= jsonObject["y"];
				var z	= jsonObject["z"];
				var w	= jsonObject["w"];

				if(x == null || y == null || z == null || w == null)
				{
					return null;
				}

				return new Vector4(x.Value<float>(),y.Value<float>(),z.Value<float>(),w.Value<float>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Vector4 vector4))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("x");	writer.WriteValue(vector4.x);
				writer.WritePropertyName("y");	writer.WriteValue(vector4.y);
				writer.WritePropertyName("z");	writer.WriteValue(vector4.z);
				writer.WritePropertyName("w");	writer.WriteValue(vector4.w);

				writer.WriteEndObject();
			}
		}
		#endregion Vector4

		#region Quaternion
		private class QuaternionJsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Quaternion);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var x 	= jsonObject["x"];
				var y	= jsonObject["y"];
				var z	= jsonObject["z"];
				var w	= jsonObject["w"];

				if(x == null || y == null || z == null || w == null)
				{
					return null;
				}

				return new Quaternion(x.Value<float>(),y.Value<float>(),z.Value<float>(),w.Value<float>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Quaternion quaternion))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("x");	writer.WriteValue(quaternion.x);
				writer.WritePropertyName("y");	writer.WriteValue(quaternion.y);
				writer.WritePropertyName("z");	writer.WriteValue(quaternion.z);
				writer.WritePropertyName("w");	writer.WriteValue(quaternion.w);

				writer.WriteEndObject();
			}
		}
		#endregion Quaternion

		#region Rect
		private class RectJsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(Rect);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var x 	= jsonObject["x"];
				var y	= jsonObject["y"];
				var w	= jsonObject["width"];
				var h	= jsonObject["heigh"];

				if(x == null || y == null || w == null || h == null)
				{
					return null;
				}

				return new Rect(x.Value<float>(),y.Value<float>(),w.Value<float>(),h.Value<float>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is Rect rect))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("x");		writer.WriteValue(rect.x);
				writer.WritePropertyName("y");		writer.WriteValue(rect.y);
				writer.WritePropertyName("width");	writer.WriteValue(rect.width);
				writer.WritePropertyName("heigh");	writer.WriteValue(rect.height);

				writer.WriteEndObject();
			}
		}
		#endregion Rect

		#region RectInt
		private class RectIntJsonConverter : JsonConverter
		{
			public override bool CanConvert(Type _type)
			{
				return _type == typeof(RectInt);
			}

			public override object? ReadJson(JsonReader reader,Type objectType,object? existingValue,JsonSerializer serializer)
			{
				var jsonObject = JObject.Load(reader);

				var x 	= jsonObject["x"];
				var y	= jsonObject["y"];
				var w	= jsonObject["width"];
				var h	= jsonObject["heigh"];

				if(x == null || y == null || w == null || h == null)
				{
					return null;
				}

				return new RectInt(x.Value<int>(),y.Value<int>(),w.Value<int>(),h.Value<int>());
			}

			public override void WriteJson(JsonWriter writer,object? value,JsonSerializer serializer)
			{
				if(!(value is RectInt rect))
				{
					writer.WriteNull();

					return;
				}

				writer.WriteStartObject();

				writer.WritePropertyName("x");		writer.WriteValue(rect.x);
				writer.WritePropertyName("y");		writer.WriteValue(rect.y);
				writer.WritePropertyName("width");	writer.WriteValue(rect.width);
				writer.WritePropertyName("heigh");	writer.WriteValue(rect.height);

				writer.WriteEndObject();
			}
		}
		#endregion RectInt
	}
}