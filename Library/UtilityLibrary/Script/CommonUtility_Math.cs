using System;

namespace KZLib
{
	public static partial class CommonUtility
	{
		#region Clamp
		public static int LoopClamp(int _index,int _size)
		{
			return _size < 1 ? 0 : _index < 0 ? _size-1+(_index+1)%_size : _index%_size;
		}

		public static float LoopClamp(float _index,int _size)
		{
			return _size < 1 ? 0 : _index < 0.0f ? _size-1+(_index+1)%_size : _index%_size;
		}

		public static TCompare Clamp<TCompare>(TCompare _curValue,TCompare _minValue,TCompare _maxValue) where TCompare : IComparable<TCompare>
		{
			return _curValue.CompareTo(_minValue) < 0 ? _minValue : _curValue.CompareTo(_maxValue) > 0 ? _maxValue : _curValue;
		}

		public static TCompare MinClamp<TCompare>(TCompare _curValue,TCompare _minValue) where TCompare : IComparable<TCompare>
		{
			return Clamp(_curValue,_minValue,_curValue);
		}

		public static TCompare MaxClamp<TCompare>(TCompare _curValue,TCompare _maxValue) where TCompare : IComparable<TCompare>
		{
			return Clamp(_curValue,_curValue,_maxValue);
		}
		#endregion Clamp
	}
}