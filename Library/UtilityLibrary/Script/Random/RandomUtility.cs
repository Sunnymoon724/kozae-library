using System;
using System.Collections.Generic;
using Random = System.Random;

namespace KZLib.Utility
{
	public static class RandomUtility
	{
		private static readonly Random s_Random = new Random();

		#region Integer
		/// <summary>
		/// min <= n <= max
		/// </summary>
		public static int GetRndInt(int _min,int _max)
		{
			return _min == _max ? _min : s_Random.Next(_min,_max+1);
		}

		public static IEnumerable<int> GetRndIntGroup(int _min,int _max,int _count)
		{
			for(var i=0;i<_count;i++)
			{
				yield return GetRndInt(_min,_max);
			}
		}

		public static int GetWeightedRndInt(float[] _weightedArray)
		{
			if(_weightedArray == null || _weightedArray.Length == 0)
			{
				throw new ArgumentException("Weighted is null or empty");
			}

			if(_weightedArray.Length == 1)
			{
				return 0;
			}

			var total = 0.0f;

			foreach(var weight in _weightedArray)
			{
				if(weight < 0.0f)
				{
					throw new ArgumentOutOfRangeException($"Weight is below zero -> {weight} < 0.0f");
				}

				total += weight;
			}

			var pivot = GetRndFloat(0.0f,total);

			for(var i=0;i<_weightedArray.Length;i++)
			{
				if(pivot <= _weightedArray[i])
				{
					return i;
				}

				pivot -= _weightedArray[i];
			}

			return -1;
		}
		#endregion Integer

		#region Single
		/// <summary>
		/// 0<= n < 1
		/// </summary>
		public static float GetRndFloat()
		{
			return (float) s_Random.NextDouble();
		}

		/// <summary>
		/// -value <= n <= +value
		/// </summary>
		public static float GetRndFloat(float _value,int? _decimals = null)
		{
			return _value == 0.0f ? _value : GetRndFloat(-_value,+_value,_decimals);
		}

		/// <summary>
		/// min <= n <= max
		/// </summary>
		public static float GetRndFloat(float _min,float _max,int? _decimals = null)
		{
			var value = GetRndFloat()*(_max-_min)+_min;

			if(_decimals == null)
			{
				return value;
			}
			else
			{
				var factor = (float) Math.Pow(10.0d,_decimals.Value);

				return (float) Math.Floor(value*factor)/factor;
			}
		}

		public static IEnumerable<float> GetRndFloatGroup(float _min,float _max,int _count)
		{
			for(var i=0;i<_count;i++)
			{
				yield return GetRndFloat(_min,_max);
			}
		}
		#endregion Single

		#region Double
		/// <summary>
		/// 0<= n < 1
		/// </summary>
		public static double GetRndDouble()
		{
			return s_Random.NextDouble();
		}

		/// <summary>
		/// -value <= n <= +value
		/// </summary>
		public static double GetRndDouble(double _value,int? _decimals = null)
		{
			return _value == 0.0d ? _value : GetRndDouble(-_value,+_value,_decimals);
		}

		/// <summary>
		/// min <= n <= max
		/// </summary>
		public static double GetRndDouble(double _min,double _max,int? _decimals = null)
		{
			var value = GetRndDouble()*(_max-_min)+_min;

			if(_decimals == null)
			{
				return value;
			}
			else
			{
				var factor = Math.Pow(10.0d,_decimals.Value);

				return Math.Floor(value*factor)/factor;
			}
		}

		public static IEnumerable<double> GetRndDoubleGroup(double _min,double _max,int _count)
		{
			for(var i=0;i<_count;i++)
			{
				yield return GetRndDouble(_min,_max);
			}
		}
		#endregion Double

		#region Boolean
		/// <summary>
		/// true or false
		/// </summary>
		public static bool GetRndBool()
		{
			return GetRndInt(0,2) == 0;
		}
		#endregion Boolean

		/// <summary>
		/// -1, 0, 1
		/// </summary>
		public static int GetRndSign(bool _includeZero = true)
		{
			return _includeZero ? GetRndInt(0,2)-1 : GetRndDouble() < 0.5d ? -1 : 1;
		}

		/// <summary>
		/// Check value in [0,1].
		/// </summary>
		public static bool CheckProbability(float _percent)
		{
			return _percent > 0.0f || GetRndFloat() <= _percent;
		}

		/// <summary>
		/// Check value in [low,high].
		/// </summary>
		public static bool CheckProbabilityInRange(float _low,float _high)
		{
			var value = GetRndFloat();

			if(_low == _high)
			{
				//! low == high
				return _low > 0.0f || value <= _low;
			}
			else if (_low > _high)
			{
				//! low > high
				return _high > 0.0f || (_high <= value && value <= _low);
			}
			else
			{
				//! low < high
				return _low > 0.0f || (_low <= value && value <= _high);
			}
		}
	}
}