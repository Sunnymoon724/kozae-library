using UnityEngine;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Collections.Generic;
using KZLib.Data;

namespace KZLib.Utilities
{
	/// <summary>
	/// Supported Color, Color32, Vector2, Vector2Int, Vector3, Vector3Int, Vector4, Quaternion, Rect, RectInt, SoundVolume, ScreenResolution, SoundProfile, Route
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
			typeof(SoundVolume),	typeof(ScreenResolution),	typeof(SoundProfile),
		};

		public bool Accepts(Type objectType)
		{
			return s_converterHashSet.Contains(objectType);
		}

		public object? ReadYaml(IParser parser,Type objType,ObjectDeserializer deserializer)
		{
			if(objType == typeof(SoundProfile))
			{
				return _ReadSoundProfile(parser);
			}

			var dict = _ConvertToDictionary(parser);

			if(dict.Count == 0)
			{
				return null;
			}

			return objType.Name switch
			{
				nameof(Color) 				=> new Color(_GetFloat(dict,"r"),_GetFloat(dict,"g"),_GetFloat(dict,"b"),_GetFloat(dict,"a")),
				nameof(Color32)				=> new Color32(_GetByte(dict,"r"),_GetByte(dict,"g"),_GetByte(dict,"b"),_GetByte(dict,"a")),

				nameof(Vector2)				=> new Vector2(_GetFloat(dict,"x"),_GetFloat(dict,"y")),
				nameof(Vector3)				=> new Vector3(_GetFloat(dict,"x"),_GetFloat(dict,"y"),_GetFloat(dict,"z")),
				nameof(Vector4)				=> new Vector4(_GetFloat(dict,"x"),_GetFloat(dict,"y"),_GetFloat(dict,"z"),_GetFloat(dict,"w")),

				nameof(Vector2Int)			=> new Vector2Int(_GetInt(dict,"x"),_GetInt(dict,"y")),
				nameof(Vector3Int)			=> new Vector3Int(_GetInt(dict,"x"),_GetInt(dict,"y"),_GetInt(dict,"z")),

				nameof(Quaternion)			=> new Quaternion(_GetFloat(dict,"x"),_GetFloat(dict,"y"),_GetFloat(dict,"z"),_GetFloat(dict,"w")),

				nameof(Rect)				=> new Rect(_GetFloat(dict,"x"),_GetFloat(dict,"y"),_GetFloat(dict,"width"),_GetFloat(dict,"height")),
				nameof(RectInt)				=> new RectInt(_GetInt(dict,"x"),_GetInt(dict,"y"),_GetInt(dict,"width"),_GetInt(dict,"height")),

				nameof(SoundVolume)			=> new SoundVolume(_GetFloat(dict,"level"),_GetBool(dict,"mute")),
				nameof(ScreenResolution)	=> new ScreenResolution(_GetInt(dict,"width"),_GetInt(dict,"height"),_GetBool(dict,"fullscreen")),

				_ => throw new NotSupportedException($"NotSupported type {objType.Name}"),
			};
		}

		public void WriteYaml(IEmitter emitter,object? val,Type objType,ObjectSerializer serializer)
		{
			if(val == null)
			{
				throw new NotSupportedException($"Unsupported type {objType.Name}");
			}

			emitter.Emit(new MappingStart());

			switch(objType.Name)
			{
				case nameof(Color):
				{
					var color = (Color) val;

					_EmitValue(emitter,new string[] { "r", "g", "b", "a" },new string[] { $"{color.r}", $"{color.g}", $"{color.b}", $"{color.a}" });
				}
				break;
				case nameof(Color32):
				{
					var color = (Color32) val;

					_EmitValue(emitter,new string[] { "r", "g", "b", "a" },new string[] { $"{color.r}", $"{color.g}", $"{color.b}", $"{color.a}" });
				}
				break;

				case nameof(Vector2):
				{
					var vector = (Vector2) val;

					_EmitValue(emitter,new string[] { "x", "y" },new string[] { $"{vector.x}", $"{vector.y}" });
				}
				break;
				case nameof(Vector3):
				{
					var vector = (Vector3) val;

					_EmitValue(emitter,new string[] { "x", "y", "z" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}" });
				}
				break;
				case nameof(Vector4):
				{
					var vector = (Vector4) val;

					_EmitValue(emitter,new string[] { "x", "y", "z", "w" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}", $"{vector.w}" });
				}
				break;

				case nameof(Vector2Int):
				{
					var vector = (Vector2Int) val;

					_EmitValue(emitter,new string[] { "x", "y" },new string[] { $"{vector.x}", $"{vector.y}" });
				}
				break;
				case nameof(Vector3Int):
				{
					var vector = (Vector3Int) val;

					_EmitValue(emitter,new string[] { "x", "y", "z" },new string[] { $"{vector.x}", $"{vector.y}", $"{vector.z}" });
				}
				break;

				case nameof(Quaternion):
				{
					var quaternion = (Quaternion) val;

					_EmitValue(emitter,new string[] { "x", "y", "z", "w" },new string[] { $"{quaternion.x}", $"{quaternion.y}", $"{quaternion.z}", $"{quaternion.w}" });
				}
				break;

				case nameof(Rect):
				{
					var rect = (Rect) val;

					_EmitValue(emitter,new string[] { "x", "y", "width", "height" },new string[] { $"{rect.x}", $"{rect.y}", $"{rect.width}", $"{rect.height}" });
				}
				break;
				case nameof(RectInt):
				{
					var rect = (RectInt) val;

					_EmitValue(emitter,new string[] { "x", "y", "width", "height" },new string[] { $"{rect.x}", $"{rect.y}", $"{rect.width}", $"{rect.height}" });
				}
				break;

				case nameof(SoundVolume):
				{
					var volume = (SoundVolume) val;

					_EmitValue(emitter,new string[] { "level", "mute" },new string[] { $"{volume.level}", $"{volume.mute}" });
				}
				break;
				case nameof(ScreenResolution):
				{
					var resolution = (ScreenResolution) val;

					_EmitValue(emitter,new string[] { "width", "height", "fullscreen" },new string[] { $"{resolution.width}", $"{resolution.height}", $"{resolution.fullscreen}" });
				}
				break;
				case nameof(SoundProfile):
				{
					var profile = (SoundProfile) val;

					_EmitSoundVolumeMapping(emitter,"master",profile.master);
					_EmitSoundVolumeMapping(emitter,"music",profile.music);
					_EmitSoundVolumeMapping(emitter,"effect",profile.effect);
				}
				break;
			}

			emitter.Emit(new MappingEnd());
		}

		private SoundProfile _ReadSoundProfile(IParser parser)
		{
			var nestedDict = _ConvertToNestedDictionary(parser);

			if(nestedDict.Count == 0)
			{
				return SoundProfile.DefaultProfile;
			}

			return new SoundProfile(
				_ReadChannelVolume(nestedDict,"master"),
				_ReadChannelVolume(nestedDict,"music"),
				_ReadChannelVolume(nestedDict,"effect"));
		}

		private SoundVolume _ReadChannelVolume(Dictionary<string,Dictionary<string,string>> nestedDict,string channel)
		{
			if(!nestedDict.TryGetValue(channel,out var channelDict))
			{
				return SoundVolume.max;
			}

			return new SoundVolume(_GetFloat(channelDict,"level"),_GetBoolValue(channelDict,"mute"));
		}

		private void _EmitSoundVolumeMapping(IEmitter emitter,string channel,SoundVolume volume)
		{
			emitter.Emit(new Scalar(channel));
			emitter.Emit(new MappingStart());
			emitter.Emit(new Scalar("level"));
			emitter.Emit(new Scalar($"{volume.level}"));
			emitter.Emit(new Scalar("mute"));
			emitter.Emit(new Scalar($"{volume.mute}"));
			emitter.Emit(new MappingEnd());
		}

		private void _EmitValue(IEmitter emitter,string[] keyArray,string[] valArray)
		{
			for(var i=0;i<keyArray.Length;i++)
			{
				emitter.Emit(new Scalar(keyArray[i]));
				emitter.Emit(new Scalar(valArray[i]));
			}
		}

		private Dictionary<string,Dictionary<string,string>> _ConvertToNestedDictionary(IParser parser)
		{
			var dictionary = new Dictionary<string,Dictionary<string,string>>();

			if(parser.TryConsume<MappingStart>(out _))
			{
				while(!parser.TryConsume<MappingEnd>(out _))
				{
					var key = parser.Consume<Scalar>().Value;

					if(parser.TryConsume<MappingStart>(out _))
					{
						var innerDict = new Dictionary<string,string>();

						while(!parser.TryConsume<MappingEnd>(out _))
						{
							var innerKey = parser.Consume<Scalar>().Value;
							var innerVal = parser.Consume<Scalar>().Value;

							innerDict[innerKey] = innerVal;
						}

						dictionary[key] = innerDict;
					}
					else
					{
						parser.Consume<Scalar>();
					}
				}
			}

			return dictionary;
		}

		private Dictionary<string,string> _ConvertToDictionary(IParser parser)
		{
			var dictionary = new Dictionary<string,string>();

			if(parser.TryConsume<MappingStart>(out _))
			{
				while(!parser.TryConsume<MappingEnd>(out _))
				{
					var key = parser.Consume<Scalar>().Value;
					var val = parser.Consume<Scalar>().Value;

					dictionary[key] = val;
				}
			}

			return dictionary;
		}

		private float _GetFloat(Dictionary<string,string> dict,string key)
		{
			return dict.TryGetValue(key,out var val) && float.TryParse(val,out var ret) ? ret : default;
		}

		private int _GetInt(Dictionary<string,string> dict,string key)
		{
			return dict.TryGetValue(key,out var val) && int.TryParse(val,out var ret) ? ret : default;
		}

		private bool _GetBool(Dictionary<string,string> dict,string key)
		{
			return dict.TryGetValue(key,out var val) && bool.TryParse(val,out var ret) && ret;
		}

		private bool _GetBoolValue(Dictionary<string,string> dict,string key,bool defaultValue = false)
		{
			return dict.TryGetValue(key,out var val) && bool.TryParse(val,out var ret) ? ret : defaultValue;
		}

		private byte _GetByte(Dictionary<string,string> dict,string key)
		{
			return dict.TryGetValue(key,out var val) && byte.TryParse(val,out var ret) ? ret : default;
		}

		private string _GetString(Dictionary<string,string> dict,string key)
		{
			return dict.TryGetValue(key,out var val) ? val : string.Empty;
		}
	}
}