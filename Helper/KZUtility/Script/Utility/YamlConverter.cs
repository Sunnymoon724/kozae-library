using UnityEngine;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Collections.Generic;
using KZLib.KZData;

namespace KZLib.KZUtility
{
	/// <summary>
	/// Supported Color, Color32, Vector2, Vector2Int, Vector3, Vector3Int, Vector4, Quaternion, Rect, RectInt, SoundVolume
	/// <br/>
	/// <b> Example </b>
	/// <br/>
	/// var serializer = new SerializerBuilder().WithTypeConverter(new YamlConverter()).Build();
	/// <br/>
	/// var text = serializer.Serialize(object);
	/// <br/>
	/// <br/>
	/// var deserializer = new DeserializerBuilder().WithTypeConverter(new YamlConverter()).Build();
	/// <br/>
	/// var object = deserializer.Serialize(text);
	/// </summary>
	public class YamlConverter : IYamlTypeConverter
	{
		private readonly static HashSet<Type> s_converter_hashSet = new HashSet<Type>
		{
			typeof(Color),		typeof(Color32),
			typeof(Vector2),	typeof(Vector3),	typeof(Vector4),
			typeof(Vector2Int),	typeof(Vector3Int),
			typeof(Quaternion),
			typeof(Rect),		typeof(RectInt),
			typeof(SoundVolume),
		};

		public bool Accepts(Type objectType)
		{
			return s_converter_hashSet.Contains(objectType);
		}

		public object? ReadYaml(IParser parser,Type objectType,ObjectDeserializer deserializer)
		{
			var dictionary = ConvertToDictionary(parser);

			if(dictionary.Count == 0)
			{
				return null;
			}

			return objectType.Name switch
			{
				nameof(Color) => new Color(GetFloat(dictionary,"r"),GetFloat(dictionary,"g"),GetFloat(dictionary,"b"),GetFloat(dictionary,"a")),
				nameof(Color32) => new Color32(GetByte(dictionary,"r"),GetByte(dictionary,"g"),GetByte(dictionary,"b"),GetByte(dictionary,"a")),

				nameof(Vector2) => new Vector2(GetFloat(dictionary,"x"),GetFloat(dictionary,"y")),
				nameof(Vector3) => new Vector3(GetFloat(dictionary,"x"),GetFloat(dictionary,"y"),GetFloat(dictionary,"z")),
				nameof(Vector4) => new Vector4(GetFloat(dictionary,"x"),GetFloat(dictionary,"y"),GetFloat(dictionary,"z"),GetFloat(dictionary,"w")),

				nameof(Vector2Int) => new Vector2Int(GetInt(dictionary,"x"),GetInt(dictionary,"y")),
				nameof(Vector3Int) => new Vector3Int(GetInt(dictionary,"x"),GetInt(dictionary,"y"),GetInt(dictionary,"z")),

				nameof(Quaternion) => new Quaternion(GetFloat(dictionary,"x"),GetFloat(dictionary,"y"),GetFloat(dictionary,"z"),GetFloat(dictionary,"w")),

				nameof(Rect) => new Rect(GetFloat(dictionary,"x"),GetFloat(dictionary,"y"),GetFloat(dictionary,"width"),GetFloat(dictionary,"heigh")),
				nameof(RectInt) => new RectInt(GetInt(dictionary,"x"),GetInt(dictionary,"y"),GetInt(dictionary,"width"),GetInt(dictionary,"heigh")),

				nameof(SoundVolume) => new SoundVolume(GetFloat(dictionary,"level"),GetBool(dictionary,"mute")),

				_ => throw new NotSupportedException($"NotSupported type {objectType.Name}"),
			};
		}

		public void WriteYaml(IEmitter emitter,object? value,Type objectType,ObjectSerializer serializer)
		{
			if(value == null)
			{
				throw new NotSupportedException($"Unsupported type {objectType.Name}");
			}

			emitter.Emit(new MappingStart());

			switch(objectType.Name)
			{
				case nameof(Color):
				{
					var color = (Color) value;

					EmitValue(emitter,new string[] { "r", "g", "b", "a" },new string[] { $"{color.r}", $"{color.g}", $"{color.b}", $"{color.a}" });
				}
				break;
				case nameof(Color32):
				{
					var color = (Color32) value;

					EmitValue(emitter,new string[] { "r", "g", "b", "a" },new string[] { $"{color.r}", $"{color.g}", $"{color.b}", $"{color.a}" });
				}
				break;

				case nameof(Vector2):
				{
					var vector = (Vector2) value;

					EmitValue(emitter,new string[] { "x", "y" },new string[] { $"{vector.x}", $"{vector.y}" });
				}
				break;
				case nameof(Vector3):
				{
					var vector = (Vector3) value;

					EmitValue(emitter,new string[] { "x", "y", "z" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}" });
				}
				break;
				case nameof(Vector4):
				{
					var vector = (Vector4) value;

					EmitValue(emitter,new string[] { "x", "y", "z", "w" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}", $"{vector.w}" });
				}
				break;

				case nameof(Vector2Int):
				{
					var vector = (Vector2Int) value;

					EmitValue(emitter,new string[] { "x", "y" },new string[] { $"{vector.x}", $"{vector.y}" });
				}
				break;
				case nameof(Vector3Int):
				{
					var vector = (Vector3Int) value;

					EmitValue(emitter,new string[] { "x", "y", "z" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}" });
				}
				break;

				case nameof(Quaternion):
				{
					var quaternion = (Quaternion) value;

					EmitValue(emitter,new string[] { "x", "y", "z", "w" },new string[] { $"{quaternion.x}", $"{quaternion.y}", $"{quaternion.z}", $"{quaternion.w}" });
				}
				break;

				case nameof(Rect):
				{
					var rect = (Rect) value;

					EmitValue(emitter,new string[] { "x", "y", "width", "height" },new string[] { $"{rect.x}", $"{rect.y}", $"{rect.width}", $"{rect.height}" });
				}
				break;
				case nameof(RectInt):
				{
					var rect = (RectInt) value;

					EmitValue(emitter,new string[] { "x", "y", "width", "height" },new string[] { $"{rect.x}", $"{rect.y}", $"{rect.width}", $"{rect.height}" });
				}
				break;

				case nameof(SoundVolume):
				{
					var volume = (SoundVolume) value;

					EmitValue(emitter,new string[] { "level", "mute" },new string[] { $"{volume.level}", $"{volume.mute}" });
				}
				break;
			}

			emitter.Emit(new MappingEnd());
		}

		private void EmitValue(IEmitter emitter,string[] keyArray,string[] valueArray)
		{
			for(var i=0;i<keyArray.Length;i++)
			{
				emitter.Emit(new Scalar(keyArray[i]));
				emitter.Emit(new Scalar(valueArray[i]));
			}
		}

		private Dictionary<string,string> ConvertToDictionary(IParser parser)
		{
			var dictionary = new Dictionary<string,string>();

			if(parser.TryConsume<MappingStart>(out _))
			{
				while(!parser.TryConsume<MappingEnd>(out _))
				{
					var key = parser.Consume<Scalar>().Value;
					var value = parser.Consume<Scalar>().Value;

					dictionary[key] = value;
				}
			}

			return dictionary;
		}

		private float GetFloat(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) && float.TryParse(value,out var result) ? result : default;
		}

		private int GetInt(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) && int.TryParse(value,out var result) ? result : default;
		}

		private bool GetBool(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) && bool.TryParse(value,out var result) && result;
		}

		private byte GetByte(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) && byte.TryParse(value,out var result) ? result : default;
		}
	}
}