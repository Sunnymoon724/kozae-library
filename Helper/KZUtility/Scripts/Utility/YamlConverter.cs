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
	/// Supported Color, Color32, Vector2, Vector2Int, Vector3, Vector3Int, Vector4, Quaternion, Rect, RectInt, SoundVolume, ScreenResolution, Route
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
		private readonly static HashSet<Type> s_converterHashSet = new()
		{
			typeof(Color),			typeof(Color32),
			typeof(Vector2),		typeof(Vector3),			typeof(Vector4),
			typeof(Vector2Int),		typeof(Vector3Int),
			typeof(Quaternion),
			typeof(Rect),			typeof(RectInt),
			typeof(SoundVolume),	typeof(ScreenResolution),
		};

		public bool Accepts(Type objectType)
		{
			return s_converterHashSet.Contains(objectType);
		}

		public object? ReadYaml(IParser parser,Type objectType,ObjectDeserializer deserializer)
		{
			var dictionary = _ConvertToDictionary(parser);

			if(dictionary.Count == 0)
			{
				return null;
			}

			return objectType.Name switch
			{
				nameof(Color) 				=> new Color(_GetFloat(dictionary,"r"),_GetFloat(dictionary,"g"),_GetFloat(dictionary,"b"),_GetFloat(dictionary,"a")),
				nameof(Color32)				=> new Color32(_GetByte(dictionary,"r"),_GetByte(dictionary,"g"),_GetByte(dictionary,"b"),_GetByte(dictionary,"a")),

				nameof(Vector2)				=> new Vector2(_GetFloat(dictionary,"x"),_GetFloat(dictionary,"y")),
				nameof(Vector3)				=> new Vector3(_GetFloat(dictionary,"x"),_GetFloat(dictionary,"y"),_GetFloat(dictionary,"z")),
				nameof(Vector4)				=> new Vector4(_GetFloat(dictionary,"x"),_GetFloat(dictionary,"y"),_GetFloat(dictionary,"z"),_GetFloat(dictionary,"w")),

				nameof(Vector2Int)			=> new Vector2Int(_GetInt(dictionary,"x"),_GetInt(dictionary,"y")),
				nameof(Vector3Int)			=> new Vector3Int(_GetInt(dictionary,"x"),_GetInt(dictionary,"y"),_GetInt(dictionary,"z")),

				nameof(Quaternion)			=> new Quaternion(_GetFloat(dictionary,"x"),_GetFloat(dictionary,"y"),_GetFloat(dictionary,"z"),_GetFloat(dictionary,"w")),

				nameof(Rect)				=> new Rect(_GetFloat(dictionary,"x"),_GetFloat(dictionary,"y"),_GetFloat(dictionary,"width"),_GetFloat(dictionary,"heigh")),
				nameof(RectInt)				=> new RectInt(_GetInt(dictionary,"x"),_GetInt(dictionary,"y"),_GetInt(dictionary,"width"),_GetInt(dictionary,"heigh")),

				nameof(SoundVolume)			=> new SoundVolume(_GetFloat(dictionary,"level"),_GetBool(dictionary,"mute")),
				nameof(ScreenResolution)	=> new ScreenResolution(_GetInt(dictionary,"width"),_GetInt(dictionary,"height"),_GetBool(dictionary,"fullscreen")),

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

					_EmitValue(emitter,new string[] { "r", "g", "b", "a" },new string[] { $"{color.r}", $"{color.g}", $"{color.b}", $"{color.a}" });
				}
				break;
				case nameof(Color32):
				{
					var color = (Color32) value;

					_EmitValue(emitter,new string[] { "r", "g", "b", "a" },new string[] { $"{color.r}", $"{color.g}", $"{color.b}", $"{color.a}" });
				}
				break;

				case nameof(Vector2):
				{
					var vector = (Vector2) value;

					_EmitValue(emitter,new string[] { "x", "y" },new string[] { $"{vector.x}", $"{vector.y}" });
				}
				break;
				case nameof(Vector3):
				{
					var vector = (Vector3) value;

					_EmitValue(emitter,new string[] { "x", "y", "z" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}" });
				}
				break;
				case nameof(Vector4):
				{
					var vector = (Vector4) value;

					_EmitValue(emitter,new string[] { "x", "y", "z", "w" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}", $"{vector.w}" });
				}
				break;

				case nameof(Vector2Int):
				{
					var vector = (Vector2Int) value;

					_EmitValue(emitter,new string[] { "x", "y" },new string[] { $"{vector.x}", $"{vector.y}" });
				}
				break;
				case nameof(Vector3Int):
				{
					var vector = (Vector3Int) value;

					_EmitValue(emitter,new string[] { "x", "y", "z" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}" });
				}
				break;

				case nameof(Quaternion):
				{
					var quaternion = (Quaternion) value;

					_EmitValue(emitter,new string[] { "x", "y", "z", "w" },new string[] { $"{quaternion.x}", $"{quaternion.y}", $"{quaternion.z}", $"{quaternion.w}" });
				}
				break;

				case nameof(Rect):
				{
					var rect = (Rect) value;

					_EmitValue(emitter,new string[] { "x", "y", "width", "height" },new string[] { $"{rect.x}", $"{rect.y}", $"{rect.width}", $"{rect.height}" });
				}
				break;
				case nameof(RectInt):
				{
					var rect = (RectInt) value;

					_EmitValue(emitter,new string[] { "x", "y", "width", "height" },new string[] { $"{rect.x}", $"{rect.y}", $"{rect.width}", $"{rect.height}" });
				}
				break;

				case nameof(SoundVolume):
				{
					var volume = (SoundVolume) value;

					_EmitValue(emitter,new string[] { "level", "mute" },new string[] { $"{volume.level}", $"{volume.mute}" });
				}
				break;
				case nameof(ScreenResolution):
				{
					var resolution = (ScreenResolution) value;

					_EmitValue(emitter,new string[] { "width", "height", "fullscreen" },new string[] { $"{resolution.width}", $"{resolution.height}", $"{resolution.fullscreen}" });
				}
				break;
			}

			emitter.Emit(new MappingEnd());
		}

		private void _EmitValue(IEmitter emitter,string[] keyArray,string[] valueArray)
		{
			for(var i=0;i<keyArray.Length;i++)
			{
				emitter.Emit(new Scalar(keyArray[i]));
				emitter.Emit(new Scalar(valueArray[i]));
			}
		}

		private Dictionary<string,string> _ConvertToDictionary(IParser parser)
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

		private float _GetFloat(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) && float.TryParse(value,out var result) ? result : default;
		}

		private int _GetInt(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) && int.TryParse(value,out var result) ? result : default;
		}

		private bool _GetBool(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) && bool.TryParse(value,out var result) && result;
		}

		private byte _GetByte(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) && byte.TryParse(value,out var result) ? result : default;
		}

		private string _GetString(Dictionary<string,string> dictionary,string key)
		{
			return dictionary.TryGetValue(key,out var value) ? value : string.Empty;
		}
	}
}