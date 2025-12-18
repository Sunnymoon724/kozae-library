using System;
using System.Collections.Generic;

namespace KZLib.KZUtility
{
	public class Randomizer
	{
		private const int c_inValid_index = -1;

		private readonly Random m_random;
		
		public Randomizer()
		{
			m_random = new Random();
		}

		public Randomizer(int seed)
		{
			m_random = new Random(seed);
		}

		#region Integer
		/// <summary>
		/// min <= n <= max
		/// </summary>
		public int PickInteger(int minValue,int maxValue)
		{
			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			return minValue == maxValue ? minValue : m_random.Next(minValue,maxValue+1);
		}

		public IEnumerable<int> PickIntegerGroup(int minValue,int maxValue,int count,bool allowDuplicate = true)
		{
			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			if(allowDuplicate)
			{
				for(var i=0;i<count;i++)
				{
					yield return PickInteger(minValue,maxValue);
				}
			}
			else
			{
				var range = maxValue-minValue+1;

				if(count > range)
				{
					throw new ArgumentOutOfRangeException($"Cannot pick {count} unique elements from a range of size {range}.");
				}

				var uniqueHashSet = new HashSet<int>();

				while(uniqueHashSet.Count < count)
				{
					var value = PickInteger(minValue,maxValue);

					if(uniqueHashSet.Add(value))
					{
						yield return value;
					}
				}
			}
		}

		public int PickWeightedInteger(float[] weightedArray)
		{
			if(weightedArray == null || weightedArray.Length == 0)
			{
				throw new ArgumentException("Weighted array is null or empty");
			}

			if(weightedArray.Length == 1)
			{
				return 0;
			}

			var total = 0.0f;

			foreach(var weight in weightedArray)
			{
				if(weight < 0.0f)
				{
					throw new ArgumentException("Weight is below zero -> {weight} < 0.0f");
				}

				total += weight;
			}

			var pivot = PickSingle(0.0f,total);

			for(var i=0;i<weightedArray.Length;i++)
			{
				if(pivot <= weightedArray[i])
				{
					return i;
				}

				pivot -= weightedArray[i];
			}

			return c_inValid_index;
		}
		#endregion Integer

		#region Single
		/// <summary>
		/// 0<= n < 1
		/// </summary>
		public float PickSingle()
		{
			return Convert.ToSingle(m_random.NextDouble());
		}

		/// <summary>
		/// -value <= n <= +value
		/// </summary>
		public float PickSingle(float value)
		{
			return value == 0.0f ? value : PickSingle(-value,+value);
		}

		/// <summary>
		/// min <= n <= max
		/// </summary>
		public float PickSingle(float minValue,float maxValue)
		{
			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			return PickSingle()*(maxValue-minValue)+minValue;
		}

		public IEnumerable<float> PickSingleGroup(float minValue,float maxValue,int count,bool allowDuplicate = true)
		{
			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			if(allowDuplicate)
			{
				for(var i=0;i<count;i++)
				{
					yield return PickSingle(minValue,maxValue);
				}
			}
			else
			{
				var range = maxValue-minValue+1;

				if(count > range)
				{
					throw new ArgumentOutOfRangeException($"Cannot pick {count} unique elements from a range of size {range}.");
				}

				var uniqueHashSet = new HashSet<float>();

				while(uniqueHashSet.Count < count)
				{
					var value = PickSingle(minValue,maxValue);

					if(uniqueHashSet.Add(value))
					{
						yield return value;
					}
				}
			}
		}
		#endregion Single

		#region Double
		/// <summary>
		/// 0<= n < 1
		/// </summary>
		public double PickDouble()
		{
			return m_random.NextDouble();
		}

		/// <summary>
		/// -value <= n <= +value
		/// </summary>
		public double PickDouble(double value)
		{
			return value == 0.0d ? value : PickDouble(-value,+value);
		}

		/// <summary>
		/// min <= n <= max
		/// </summary>
		public double PickDouble(double minValue,double maxValue)
		{
			return PickDouble()*(maxValue-minValue)+minValue;
		}

		public IEnumerable<double> PickDoubleGroup(double minValue,double maxValue,int count,bool allowDuplicate = true)
		{
			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			if(allowDuplicate)
			{
				for(var i=0;i<count;i++)
				{
					yield return PickDouble(minValue,maxValue);
				}
			}
			else
			{
				var range = maxValue-minValue+1;

				if(count > range)
				{
					throw new ArgumentOutOfRangeException($"Cannot pick {count} unique elements from a range of size {range}.");
				}

				var uniqueHashSet = new HashSet<double>();

				while(uniqueHashSet.Count < count)
				{
					var value = PickDouble(minValue,maxValue);

					if(uniqueHashSet.Add(value))
					{
						yield return value;
					}
				}
			}
		}
		#endregion Double

		#region Boolean
		/// <summary>
		/// true or false
		/// </summary>
		public bool PickBoolean()
		{
			return PickInteger(0,2) == 0;
		}
		#endregion Boolean

		/// <summary>
		/// Check value in [0,1].
		/// </summary>
		public bool HitRate(float percent)
		{
			return PickSingle() <= percent;
		}

		/// <summary>
		/// Check value in [minValue,maxValue].
		/// </summary>
		public bool HitRateInRange(float minValue,float maxValue)
		{
			var value = PickSingle();

			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			return minValue <= value && value <= maxValue;
		}
		
		/// <summary>
		/// Check value in [0,1].
		/// </summary>
		public bool HitRate(double percent)
		{
			return PickDouble() <= percent;
		}

		/// <summary>
		/// Check value in [minValue,maxValue].
		/// </summary>
		public bool HitRateInRange(double minValue,double maxValue)
		{
			var value = PickDouble();

			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			return minValue <= value && value <= maxValue;
		}

		private CompareValue<TValue> _NormalizeRange<TValue>(TValue minValue,TValue maxValue) where TValue : IComparable<TValue>
		{
			return minValue.CompareTo(maxValue) > 0 ? new CompareValue<TValue>(maxValue,minValue) : new CompareValue<TValue>(minValue,maxValue);
		}

		private record CompareValue<TValue>(TValue minValue,TValue maxValue);
	}
}