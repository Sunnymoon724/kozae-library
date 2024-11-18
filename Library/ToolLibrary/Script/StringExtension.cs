// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.IO;
// using System.Linq;
// using System.Numerics;
// using System.Text;
// using System.Text.RegularExpressions;
// using System.Xml;
// using Newtonsoft.Json;

// public static class StringExtension
// {
// 	#region Normalize
// 	public static string NormalizeNewLines(this string _text)
// 	{
// 		return _text.Replace("\\n",Environment.NewLine);
// 	}

// 	/// <summary>
// 	/// Change path slash
// 	/// </summary>
// 	public static string PathConvertSlash(this string _path)
// 	{
// #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
// 		return _path.Replace('/',Path.DirectorySeparatorChar);
// #elif UNITY_EDITOR_OSX || UNITY_Standalone_OSX || UNITY_IOS || UNITY_ANDROID
// 		return _path.Replace('\\',Path.DirectorySeparatorChar);
// #else
// 		return _path.Replace('\\','/');
// #endif
// 	}
// 	#endregion Normalize

// 	#region Encoding
// 	public static string CP949ToUTF8(this string _text)
// 	{
// 		var cp949 = Encoding.GetEncoding("ks_c_5601-1987");
// 		var utf8 = Encoding.UTF8;

// 		return utf8.GetString(Encoding.Convert(cp949,utf8,cp949.GetBytes(_text)));
// 	}
// 	#endregion Encoding

// 	#region Character
// 	/// <summary>
// 	/// Check character in string
// 	/// </summary>
// 	public static bool IsContainsCharacterArray(this string _text,params char[] _characterArray)
// 	{
// 		if(_text.IsEmpty() || _characterArray.IsNullOrEmpty())
// 		{
// 			return false;
// 		}

// 		var characterSet = new HashSet<char>(_characterArray);

// 		return _text.Any(x => characterSet.Contains(x));
// 	}
// 	/// <summary>
// 	/// Convert first character to uppercase
// 	/// </summary>
// 	public static string ToFirstCharacterToUpper(this string _text)
// 	{
// 		if(_text.IsEmpty())
// 		{
// 			return _text;
// 		}

// 		var text = char.ToUpperInvariant(_text[0]);

// 		return _text.Length > 1 ? $"{text}{_text[1..]}" : text.ToString();
// 	}

// 	/// <summary>
// 	/// Convert first character to lowercase
// 	/// </summary>
// 	public static string ToFirstCharacterToLower(this string _text)
// 	{
// 		if(_text.IsEmpty())
// 		{
// 			return _text;
// 		}

// 		var text = char.ToLowerInvariant(_text[0]);

// 		return _text.Length > 1 ? $"{text}{_text[1..]}" : text.ToString();
// 	}

// 	/// <summary>
// 	/// Get character 0 to count from first
// 	/// </summary>
// 	public static string GetStartCharacter(this string _text,int _count)
// 	{
// 		return _text.IsEmpty() || _count <= 0 ? _text : _text[..Mathf.Min(_count, _text.Length)];
// 	}

// 	/// <summary>
// 	/// Get character 0 to count from last
// 	/// </summary>
// 	public static string GetEndCharacter(this string _text,int _count)
// 	{
// 		return _text.IsEmpty() || _count <= 0 ? _text : _text[^(Mathf.Min(_count,_text.Length))..];
// 	}

// 	/// <summary>
// 	/// Count character in string
// 	/// </summary>
// 	public static int CountOf(this string _text,char _character)
// 	{
// 		return _text.Count(x => x == _character);
// 	}

// 	/// <summary>
// 	/// Get index of character in string
// 	/// </summary>
// 	public static int IndexOfOrder(this string _text,char _character,int _order)
// 	{
// 		if(_text.IsEmpty())
// 		{
// 			return -1;
// 		}

// 		var index = -1;

// 		for(var i=0;i<_order;i++)
// 		{
// 			index = _text.IndexOf(_character,index+1);

// 			if(index == -1)
// 			{
// 				return -1;
// 			}
// 		}
// 		return index;
// 	}
// 	#endregion Character

// 	#region Compare
// 	/// <summary>
// 	/// Check empty text
// 	/// </summary>
// 	/// <param name="_text"></param>
// 	public static bool IsEmpty(this string _text,bool _includeSpace = false)
// 	{
// 		return _includeSpace ? string.IsNullOrWhiteSpace(_text) : string.IsNullOrEmpty(_text);
// 	}

// 	public static bool IsEqual(this string _text1,string _text2)
// 	{
// 		return string.Equals(_text1,_text2);
// 	}

// 	/// <summary>
// 	/// Check match at index
// 	/// </summary>
// 	public static bool IsMatchAt(this string _text,int _index,string _match,bool _ignoreCase = false)
// 	{
// 		if(_text.IsEmpty() || _match.IsEmpty() || _index < 0 || _index+_match.Length > _text.Length)
// 		{
// 			return false;
// 		}

// 		return _ignoreCase ? string.Equals(_text.Substring(_index,_match.Length),_match,StringComparison.OrdinalIgnoreCase) : _text.Substring(_index,_match.Length) == _match;
// 	}
// 	#endregion Compare

// 	#region Convert Enum
// 	public static bool IsEnumDefined<TEnum>(this string _text)
// 	{
// 		return Enum.IsDefined(typeof(TEnum),_text);
// 	}

// 	public static TEnum ToEnum<TEnum>(this string _text,TEnum _default = default) where TEnum : struct
// 	{
// 		return !_text.IsEmpty() && Enum.TryParse(_text,true,out TEnum data) ? data : _default;
// 	}
// 	#endregion Convert Enum

// 	#region Convert Color
// 	/// <summary>
// 	/// HexCode to Color
// 	/// </summary>
// 	public static Color ToColor(this string _hexCode)
// 	{
// 		if(_hexCode.Length == 7)
// 		{
// 			_hexCode = $"{_hexCode}FF";
// 		}

// 		if(s_HexColorDict.TryGetValue(_hexCode,out var color))
// 		{
// 			return color;
// 		}

// 		if(ColorUtility.TryParseHtmlString(_hexCode,out color))
// 		{
// 			s_HexColorDict.Add(_hexCode,color);

// 			return color;
// 		}

// 		return Color.clear;
// 	}

// 	/// <summary>
// 	/// Add richText
// 	/// </summary>
// 	public static string ToColorText(this string _text,string _color)
// 	{
// 		return $"<color=#{_color}>{_text}</color>";
// 	}

// 	/// <summary>
// 	/// Add richText
// 	/// </summary>
// 	public static string ToColorText(this string _text,Color _color)
// 	{
// 		return ToColorText(ColorUtility.ToHtmlStringRGBA(_color),_text);
// 	}
// 	#endregion Convert Color

// 	#region Convert Number
// 	public static BigInteger ToBigInteger(this string _text,BigInteger _default = default)
// 	{
// 		if(_text.IsEmpty())
// 		{
// 			return _default;
// 		}

// 		return BigInteger.TryParse(_text,out var result) ? result : _default;
// 	}

// 	public static int ToInt(this string _text,int _default = 0)
// 	{
// 		if(_text.IsEmpty())
// 		{
// 			return _default;
// 		}

// 		if(_text.StartsWith("0x",StringComparison.OrdinalIgnoreCase))
// 		{
// 			return _text.ToHexInt(_default);
// 		}

// 		return int.TryParse(_text,out var num) ? num : _default;
// 	}

// 	public static float ToFloat(this string _text,float _default = 0.0f)
// 	{
// 		return float.TryParse(_text,out var num) ? num : _default;
// 	}

// 	public static double ToDouble(this string _text,double _default = 0.0d)
// 	{
// 		return double.TryParse(_text,out var num) ? num : _default;
// 	}

// 	public static byte ToByte(this string _text,byte _default = 0x00)
// 	{
// 		return _text.StartsWith("0x",StringComparison.OrdinalIgnoreCase) ? Convert.ToByte(_text,16) : byte.TryParse(_text,out var num) ? num : _default;
// 	}

// 	public static int ToHexInt(this string _hexText,int _default = 0)
// 	{
// 		return int.TryParse(_hexText,NumberStyles.HexNumber,CultureInfo.CurrentCulture,out var num) ? num : _default;
// 	}

// 	public static float ToHexFloat(string _hexText,float _default = 0.0f)
// 	{
// 		return uint.TryParse(_hexText,NumberStyles.AllowHexSpecifier,CultureInfo.CurrentCulture,out var num) ? BitConverter.ToSingle(BitConverter.GetBytes(num),0) : _default;
// 	}

// 	private static float GetNumberInArray(string[] _textArray,int _index)
// 	{
// 		return _textArray.ContainsIndex(_index) ? _textArray[_index].ToFloat() : 0.0f;
// 	}
// 	#endregion Convert Number

// 	#region Convert DateTime
// 	public static DateTime ToDateTime(this string _text,CultureInfo _cultureInfo = null, DateTimeStyles _styles = DateTimeStyles.AdjustToUniversal)
// 	{
// 		var cultureInfo = _cultureInfo ?? CultureInfo.CreateSpecificCulture("ko-KR");

// 		return DateTime.ParseExact(_text,"yyyy-MM-dd HH:mm",cultureInfo,_styles);
// 	}
// 	#endregion Convert DateTime

// 	#region Convert Vector
// 	public static Vector2 ToVector2(this string _text)
// 	{
// 		return _text.ToVector2(Vector2.zero);
// 	}

// 	public static Vector2 ToVector2(this string _text,Vector2 _default)
// 	{
// 		return _text.TryToVector2(out var result) ? result : _default;
// 	}

// 	public static bool TryToVector2(this string _text,out Vector2 _result)
// 	{
// 		_result = Vector2.zero;

// 		var vectorArray = ConvertVectorArray(_text);		

// 		if(vectorArray.IsNullOrEmpty())
// 		{
// 			return false;
// 		}

// 		_result = new Vector2(GetNumberInArray(vectorArray,0),GetNumberInArray(vectorArray,1));

// 		return true;
// 	}

// 	public static Vector3 ToVector3(this string _text)
// 	{
// 		return _text.ToVector3(Vector3.zero);
// 	}

// 	public static Vector3 ToVector3(this string _text,Vector3 _default)
// 	{
// 		return _text.TryToVector3(out var result) ? result : _default;
// 	}

// 	public static bool TryToVector3(this string _text,out Vector3 _result)
// 	{
// 		_result = Vector3.zero;

// 		var vectorArray = ConvertVectorArray(_text);

// 		if(vectorArray.IsNullOrEmpty())
// 		{
// 			return false;
// 		}

// 		_result = new Vector3(GetNumberInArray(vectorArray,0),GetNumberInArray(vectorArray,1),GetNumberInArray(vectorArray,2));

// 		return true;
// 	}

// 	private static string[] ConvertVectorArray(string _text)
// 	{
// 		return _text.IsEmpty() ? null : _text.TrimParentheses().Split(',');
// 	}
// 	#endregion Convert Vector

// 	#region Remove
// 	/// <summary>
// 	/// Remove text from first
// 	/// </summary>
// 	public static string RemoveStart(this string _text,string _remove)
// 	{
// 		return (_text.IsEmpty() || _remove.IsEmpty()) ? _text : _text.StartsWith(_remove) ? _text[_remove.Length..] : _text;
// 	}

// 	/// <summary>
// 	/// Remove text from last
// 	/// </summary>
// 	public static string RemoveEnd(this string _text,string _remove)
// 	{
// 		return (_text.IsEmpty() || _remove.IsEmpty()) ? _text : _text.EndsWith(_remove) ? _text[..^_remove.Length] : _text;
// 	}

// 	/// <summary>
// 	/// Remove text by count from first
// 	/// </summary>
// 	public static string RemoveStartCharacter(this string _text,int _count)
// 	{
// 		return (_text.IsEmpty() || _count <= 0) ? _text : _count >= _text.Length ? string.Empty : _text[_count..];
// 	}

// 	/// <summary>
// 	/// Remove text by count from last
// 	/// </summary>
// 	public static string RemoveEndCharacter(this string _text,int _count)
// 	{
// 		return (_text.IsEmpty() || _count <= 0) ? _text : _count >= _text.Length ? string.Empty : _text[..^_count];
// 	}

// 	/// <summary>
// 	/// Remove richText
// 	/// </summary>
// 	public static string RemoveRichText(this string _text)
// 	{
// 		return Regex.Replace(_text,"<.*?>",string.Empty);
// 	}
// 	#endregion Remove

// 	#region Trim
// 	/// <summary>
// 	/// Trim text from first
// 	/// </summary>
// 	public static string TrimTextStart(this string _text,string _trim)
// 	{
// 		if(_text.IsEmpty() || _trim.IsEmpty())
// 		{
// 			return _text;
// 		}

// 		while(_text.StartsWith(_trim))
// 		{
// 			_text = _text[_trim.Length..];
// 		}

// 		return _text;
// 	}

// 	/// <summary>
// 	/// Trim text from last
// 	/// </summary>
// 	public static string TrimTextEnd(this string _text,string _trim)
// 	{
// 		if(_text.IsEmpty() || _trim.IsEmpty())
// 		{
// 			return _text;
// 		}

// 		while(_text.EndsWith(_trim))
// 		{
// 			_text = _text[..^_trim.Length];
// 		}

// 		return _text;
// 	}

// 	/// <summary>
// 	/// Trim all space
// 	/// </summary>
// 	public static string TrimAllSpace(this string _text)
// 	{
//         return _text.IsEmpty() ? _text : _text.Replace(" ",string.Empty);
// 	}

// 	/// <summary>
// 	/// Trim all brackets
// 	/// </summary>
// 	public static string TrimBrackets(this string _text)
// 	{
// 		return _text.Trim('(',')','[',']','{','}','<','>');
// 	}
// 	public static string TrimParentheses(this string _text)
// 	{
// 		return _text.Trim('(',')');
// 	}
// 	public static string TrimBraces(this string _text)
// 	{
// 		return _text.Trim('{','}');
// 	}
// 	public static string TrimSquareBrackets(this string _text)
// 	{
// 		return _text.Trim('[',']');
// 	}
// 	public static string TrimAngleBrackets(this string _text)
// 	{
// 		return _text.Trim('<','>');
// 	}
// 	#endregion Trim

// 	#region Wrap
// 	public static string WrapParentheses(this string _text)
// 	{
// 		return $"({_text})";
// 	}
// 	public static string WrapBraces(this string _text)
// 	{
// 		return $"{{{_text}}}";
// 	}
// 	public static string WrapSquareBrackets(this string _text)
// 	{
// 		return $"[{_text}]";
// 	}
// 	public static string WrapAngleBrackets(this string _text)
// 	{
// 		return $"<{_text}>";
// 	}
// 	#endregion Wrap

// 	#region Split
// 	public static string[] SplitTextByBrackets(this string _text,StringSplitOptions _options = StringSplitOptions.None)
// 	{
// 		return _text.Split(new char[] { '(', ')', '[', ']', '{', '}', '<', '>' }, _options);
// 	}

// 	public static string[] SplitTextByParentheses(this string _text,StringSplitOptions _options = StringSplitOptions.None)
// 	{
// 		return _text.Split(new char[] { '(', ')' }, _options);
// 	}

// 	public static string[] SplitTextByBraces(this string _text,StringSplitOptions _options = StringSplitOptions.None)
// 	{
// 		return _text.Split(new char[] { '{', '}' }, _options);
// 	}

// 	public static string[] SplitTextBySquareBrackets(this string _text,StringSplitOptions _options = StringSplitOptions.None)
// 	{
// 		return _text.Split(new char[] { '[', ']' }, _options);
// 	}

// 	public static string[] SplitTextByAngleBrackets(this string _text,StringSplitOptions _options = StringSplitOptions.None)
// 	{
// 		return _text.Split(new char[] { '<', '>' }, _options);
// 	}
// 	#endregion Split

// 	#region Extract
// 	/// <summary>
// 	/// Extract start to end
// 	/// </summary>
// 	public static string ExtractData(this string _text,string _startText,string _endText)
// 	{
// 		var start = _text.RemoveStart(_startText);

// 		if(_text.IsEqual(start))
// 		{
// 			return _text;
// 		}

// 		var end = start.RemoveEnd(_endText);

// 		return start.IsEqual(end) ? _text : end;
// 	}

// 	/// <summary>
// 	/// Extract start to end
// 	/// </summary>
// 	public static string ExtractData(this string _text,char _startMark,char _endMark)
// 	{
// 		return _text.ExtractData($"{_startMark}",$"{_endMark}");
// 	}

// 	/// <summary>
// 	/// Extract start to end
// 	/// </summary>
// 	public static string ExtractData(this string _text,char _startMark,string _endText)
// 	{
// 		return _text.ExtractData($"{_startMark}",_endText);
// 	}

// 	/// <summary>
// 	/// Extract start to end
// 	/// </summary>
// 	public static string ExtractData(this string _text,string _startText,char _endMark)
// 	{
// 		return _text.ExtractData(_startText,$"{_endMark}");
// 	}

// 	/// <summary>
// 	/// Extract Letters
// 	/// </summary>
// 	public static string ExtractOnlyLetters(this string _text)
// 	{
// 		return _text.IsEmpty() ? null : Regex.Replace(_text,@"[^a-zA-Z]",string.Empty);
// 	}

// 	/// <summary>
// 	/// Extract Digits
// 	/// </summary>
// 	public static string ExtractOnlyDigits(this string _text)
// 	{
// 		return _text.IsEmpty() ? null : Regex.Replace(_text,@"\D",string.Empty);
// 	}

// 	/// <summary>
// 	/// Extract Alphanumeric
// 	/// </summary>
// 	public static string ExtractAlphanumeric(this string _text)
// 	{
// 		return _text.IsEmpty() ? null : Regex.Replace(_text,@"[^0-9a-zA-Z_]+",string.Empty);
// 	}

// 	public static int ExtractOnlyDigitsToInt(this string _text,int _default)
// 	{
// 		return ExtractOnlyDigits(_text).ToInt(_default);
// 	}
// 	#endregion Extract

// 	#region Layer
// 	public static int GetLayerByName(this string _layerName,bool _autoCreate = false)
// 	{
// 		var layer = LayerMask.NameToLayer(_layerName);

// #if UNITY_EDITOR
// 		if(layer == -1 && _autoCreate)
// 		{
// 			CommonUtility.AddLayer(_layerName);

// 			layer = LayerMask.NameToLayer(_layerName);
// 		}
// #endif

// 		return layer;
// 	}
// 	#endregion Layer

// 	public static string XmlToJson(this string _xml)
// 	{
// 		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(_xml));
// 		var xml = new XmlDocument();
// 		xml.Load(stream);

// 		return JsonConvert.SerializeXmlNode(xml);
// 	}
// }