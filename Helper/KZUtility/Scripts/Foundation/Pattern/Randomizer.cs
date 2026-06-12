using System;
using System.Collections.Generic;

namespace KZLib.Utilities
{
	/// <summary>
	/// Thread-safe wrapper around <see cref="Random"/> for common sampling helpers.
	/// </summary>
	/// <remarks>
	/// All random draws synchronize on an internal lock. Unique group sampling is supported only for integers via <see cref="PickIntegerGroup"/>.
	/// </remarks>
	public class Randomizer
	{
		private readonly object m_syncRoot = new();
		private readonly Random m_random = null!;

		/// <summary>
		/// Creates a randomizer with a time-based seed.
		/// </summary>
		public Randomizer()
		{
			m_random = new Random();
		}

		/// <summary>
		/// Creates a randomizer with the given seed.
		/// </summary>
		/// <param name="seed">Seed value for the internal <see cref="Random"/> instance.</param>
		public Randomizer(int seed)
		{
			m_random = new Random(seed);
		}

		#region Integer
		/// <summary>
		/// Returns a random integer in the inclusive range [minValue, maxValue].
		/// </summary>
		/// <param name="minVal">Minimum value.</param>
		/// <param name="maxVal">Maximum value.</param>
		public int PickInteger(int minVal,int maxVal)
		{
			(minVal,maxVal) = _NormalizeRange(minVal,maxVal);

			lock(m_syncRoot)
			{
				return minVal == maxVal ? minVal : m_random.Next(minVal,maxVal+1);
			}
		}

		/// <summary>
		/// Returns random integers in the inclusive range [minValue, maxValue].
		/// </summary>
		/// <param name="minVal">Minimum value.</param>
		/// <param name="maxVal">Maximum value.</param>
		/// <param name="count">Number of values to return.</param>
		/// <param name="allowDuplicate">When false, returns unique integers using partial shuffle.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative or exceeds the available unique range.</exception>
		public IEnumerable<int> PickIntegerGroup(int minVal,int maxVal,int count,bool allowDuplicate = true)
		{
			if(count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count),count,"Count must be zero or greater.");
			}

			(minVal,maxVal) = _NormalizeRange(minVal,maxVal);

			if(count == 0)
			{
				yield break;
			}

			if(allowDuplicate)
			{
				for(var i=0;i<count;i++)
				{
					yield return PickInteger(minVal,maxVal);
				}

				yield break;
			}

			var range = maxVal-minVal+1;

			if(count > range)
			{
				throw new ArgumentOutOfRangeException(nameof(count),count,$"Cannot pick {count} unique elements from a range of size {range}.");
			}

			var pool = new List<int>(range);

			for(var val=minVal;val<=maxVal;val++)
			{
				pool.Add(val);
			}

			for(var i=0;i<count;i++)
			{
				var idx = PickInteger(i,pool.Count-1);

                (pool[i],pool[idx]) = (pool[idx],pool[i]);

                yield return pool[i];
			}
		}

		/// <summary>
		/// Returns an index selected by the given weight table.
		/// </summary>
		/// <param name="weightedArray">Non-negative weights. Index probability is proportional to each weight.</param>
		/// <returns>Selected index in <paramref name="weightedArray"/>.</returns>
		/// <exception cref="ArgumentException">Thrown when the array is null, empty, contains negative weights, or sums to zero.</exception>
		public int PickWeightedInteger(float[] weightedArray)
		{
			if(weightedArray == null || weightedArray.Length == 0)
			{
				throw new ArgumentException("Weighted array is null or empty.",nameof(weightedArray));
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
					throw new ArgumentException($"Weight is below zero -> {weight} < 0.0f",nameof(weightedArray));
				}

				total += weight;
			}

			if(total <= 0.0f)
			{
				throw new ArgumentException("Total weight must be greater than zero.",nameof(weightedArray));
			}

			var roll = PickSingle()*total;
			var cumulative = 0.0f;

			for(var i=0;i<weightedArray.Length;i++)
			{
				cumulative += weightedArray[i];

				if(roll < cumulative)
				{
					return i;
				}
			}

			return weightedArray.Length-1;
		}
		#endregion Integer

		#region Single
		/// <summary>
		/// Returns a random float in the half-open range [0, 1).
		/// </summary>
		public float PickSingle()
		{
			lock(m_syncRoot)
			{
				return Convert.ToSingle(m_random.NextDouble());
			}
		}

		/// <summary>
		/// Returns a random float in the inclusive range [-value, value].
		/// </summary>
		/// <param name="val">Absolute range boundary.</param>
		public float PickSingle(float val)
		{
			return val == 0.0f ? val : PickSingle(-val,+val);
		}

		/// <summary>
		/// Returns a random float in the inclusive range [minValue, maxValue].
		/// </summary>
		/// <param name="minVal">Minimum value.</param>
		/// <param name="maxVal">Maximum value.</param>
		public float PickSingle(float minVal,float maxVal)
		{
			(minVal,maxVal) = _NormalizeRange(minVal,maxVal);

			return PickSingle()*(maxVal-minVal)+minVal;
		}

		/// <summary>
		/// Returns random floats in the inclusive range [minValue, maxValue].
		/// Unique sampling is not supported for floating-point ranges.
		/// </summary>
		/// <param name="minVal">Minimum value.</param>
		/// <param name="maxVal">Maximum value.</param>
		/// <param name="count">Number of values to return.</param>
		/// <param name="allowDuplicate">Must be true. Use <see cref="PickIntegerGroup"/> for unique sampling.</param>
		/// <exception cref="NotSupportedException">Thrown when <paramref name="allowDuplicate"/> is false.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
		public IEnumerable<float> PickSingleGroup(float minVal,float maxVal,int count,bool allowDuplicate = true)
		{
			_ThrowIfUniqueGroupIsUnsupported(allowDuplicate);

			return PickSingleGroupCore(minVal,maxVal,count);
		}

		/// <summary>Shared implementation for <see cref="PickSingleGroup"/>.</summary>
		private IEnumerable<float> PickSingleGroupCore(float minVal,float maxVal,int count)
		{
			if(count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count),count,"Count must be zero or greater.");
			}

			(minVal,maxVal) = _NormalizeRange(minVal,maxVal);

			for(var i=0;i<count;i++)
			{
				yield return PickSingle(minVal,maxVal);
			}
		}
		#endregion Single

		#region Double
		/// <summary>
		/// Returns a random double in the half-open range [0, 1).
		/// </summary>
		public double PickDouble()
		{
			lock(m_syncRoot)
			{
				return m_random.NextDouble();
			}
		}

		/// <summary>
		/// Returns a random double in the inclusive range [-value, value].
		/// </summary>
		/// <param name="val">Absolute range boundary.</param>
		public double PickDouble(double val)
		{
			return val == 0.0d ? val : PickDouble(-val,+val);
		}

		/// <summary>
		/// Returns a random double in the inclusive range [minValue, maxValue].
		/// </summary>
		/// <param name="minVal">Minimum value.</param>
		/// <param name="maxVal">Maximum value.</param>
		public double PickDouble(double minVal,double maxVal)
		{
			(minVal,maxVal) = _NormalizeRange(minVal,maxVal);

			return PickDouble()*(maxVal-minVal)+minVal;
		}

		/// <summary>
		/// Returns random doubles in the inclusive range [minValue, maxValue].
		/// Unique sampling is not supported for floating-point ranges.
		/// </summary>
		/// <param name="minVal">Minimum value.</param>
		/// <param name="maxVal">Maximum value.</param>
		/// <param name="count">Number of values to return.</param>
		/// <param name="allowDuplicate">Must be true. Use <see cref="PickIntegerGroup"/> for unique sampling.</param>
		/// <exception cref="NotSupportedException">Thrown when <paramref name="allowDuplicate"/> is false.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
		public IEnumerable<double> PickDoubleGroup(double minVal,double maxVal,int count,bool allowDuplicate = true)
		{
			_ThrowIfUniqueGroupIsUnsupported(allowDuplicate);

			return PickDoubleGroupCore(minVal,maxVal,count);
		}

		/// <summary>Shared implementation for <see cref="PickDoubleGroup"/>.</summary>
		private IEnumerable<double> PickDoubleGroupCore(double minVal,double maxVal,int count)
		{
			if(count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count),count,"Count must be zero or greater.");
			}

			(minVal,maxVal) = _NormalizeRange(minVal,maxVal);

			for(var i=0;i<count;i++)
			{
				yield return PickDouble(minVal,maxVal);
			}
		}
		#endregion Double

		#region Boolean
		/// <summary>
		/// Returns true or false with equal probability.
		/// </summary>
		public bool PickBoolean()
		{
			return PickInteger(0,1) == 0;
		}
		#endregion Boolean

		/// <summary>
		/// Returns true when a random value in [0, 1) is less than or equal to <paramref name="percent"/>.
		/// </summary>
		/// <param name="percent">Success rate in the range [0, 1].</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="percent"/> is outside [0, 1].</exception>
		public bool HitRate(float percent)
		{
			_ValidateUnitRate(percent,nameof(percent));

			return PickSingle() <= percent;
		}

		/// <summary>
		/// Returns true when a random value in [0, 1) falls within [minValue, maxValue].
		/// </summary>
		/// <param name="minVal">Lower bound in the range [0, 1].</param>
		/// <param name="maxVal">Upper bound in the range [0, 1].</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when either bound is outside [0, 1].</exception>
		public bool HitRateInRange(float minVal,float maxVal)
		{
			(minVal,maxVal) = _NormalizeRange(minVal,maxVal);
			_ValidateUnitRate(minVal,nameof(minVal));
			_ValidateUnitRate(maxVal,nameof(maxVal));

			var val = PickSingle();

			return minVal <= val && val <= maxVal;
		}

		/// <summary>
		/// Returns true when a random value in [0, 1) is less than or equal to <paramref name="percent"/>.
		/// </summary>
		/// <param name="percent">Success rate in the range [0, 1].</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="percent"/> is outside [0, 1].</exception>
		public bool HitRate(double percent)
		{
			_ValidateUnitRate(percent,nameof(percent));

			return PickDouble() <= percent;
		}

		/// <summary>
		/// Returns true when a random value in [0, 1) falls within [minValue, maxValue].
		/// </summary>
		/// <param name="minVal">Lower bound in the range [0, 1].</param>
		/// <param name="maxVal">Upper bound in the range [0, 1].</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when either bound is outside [0, 1].</exception>
		public bool HitRateInRange(double minVal,double maxVal)
		{
			(minVal,maxVal) = _NormalizeRange(minVal,maxVal);
			_ValidateUnitRate(minVal,nameof(minVal));
			_ValidateUnitRate(maxVal,nameof(maxVal));

			var val = PickDouble();

			return minVal <= val && val <= maxVal;
		}

		/// <summary>Rejects unique sampling for floating-point group methods.</summary>
		private static void _ThrowIfUniqueGroupIsUnsupported(bool allowDuplicate)
		{
			if(!allowDuplicate)
			{
				throw new NotSupportedException("Unique sampling is only supported by PickIntegerGroup.");
			}
		}

		/// <summary>Validates that a probability value is in [0, 1].</summary>
		private static void _ValidateUnitRate(float val,string paramName)
		{
			if(val < 0.0f || val > 1.0f)
			{
				throw new ArgumentOutOfRangeException(paramName,val,"Value must be in the range [0, 1].");
			}
		}

		/// <summary>Validates that a probability value is in [0, 1].</summary>
		private static void _ValidateUnitRate(double val,string paramName)
		{
			if(val < 0.0d || val > 1.0d)
			{
				throw new ArgumentOutOfRangeException(paramName,val,"Value must be in the range [0, 1].");
			}
		}

		/// <summary>
		/// Swaps bounds when <paramref name="minVal"/> is greater than <paramref name="maxVal"/>.
		/// </summary>
		private CompareValue<TValue> _NormalizeRange<TValue>(TValue minVal,TValue maxVal) where TValue : IComparable<TValue>
		{
			return minVal.CompareTo(maxVal) > 0 ? new CompareValue<TValue>(maxVal,minVal) : new CompareValue<TValue>(minVal,maxVal);
		}

		/// <summary>Normalized min/max pair.</summary>
		private record CompareValue<TValue>(TValue minVal,TValue maxVal);
	}
}
