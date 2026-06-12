using System;
using System.Collections.Generic;
using KZLib.Utilities;

/// <summary>
/// Static random helpers backed by a shared <see cref="Randomizer"/> instance.
/// </summary>
public static class KZRandomKit
{
	private static readonly Randomizer s_randomizer = new();

	/// <summary>
	/// Returns a random integer in [<paramref name="minValue"/>, <paramref name="maxValue"/>] (inclusive).
	/// </summary>
	public static int PickInteger(int minValue,int maxValue)
	{
		return s_randomizer.PickInteger(minValue,maxValue);
	}

	/// <summary>
	/// Returns a random float in [0, 1).
	/// </summary>
	public static float PickSingle()
	{
		return s_randomizer.PickSingle();
	}

	/// <summary>
	/// Returns a random float in [-<paramref name="value"/>, +<paramref name="value"/>].
	/// </summary>
	public static float PickSingle(float value)
	{
		return s_randomizer.PickSingle(value);
	}

	/// <summary>
	/// Returns a random float in [<paramref name="minValue"/>, <paramref name="maxValue"/>].
	/// </summary>
	public static float PickSingle(float minValue,float maxValue)
	{
		return s_randomizer.PickSingle(minValue,maxValue);
	}

	/// <summary>
	/// Returns a random double in [0, 1).
	/// </summary>
	public static double PickDouble()
	{
		return s_randomizer.PickDouble();
	}

	/// <summary>
	/// Returns a random double in [-<paramref name="value"/>, +<paramref name="value"/>].
	/// </summary>
	public static double PickDouble(double value)
	{
		return s_randomizer.PickDouble(value);
	}

	/// <summary>
	/// Returns a random double in [<paramref name="minValue"/>, <paramref name="maxValue"/>].
	/// </summary>
	public static double PickDouble(double minValue,double maxValue)
	{
		return s_randomizer.PickDouble(minValue,maxValue);
	}

	/// <summary>
	/// Returns true or false with equal probability.
	/// </summary>
	public static bool PickBoolean()
	{
		return s_randomizer.PickBoolean();
	}

	#region Gaussian
	/// <summary>
	/// Returns a random value from a normal distribution with <paramref name="mean"/> and <paramref name="deviation"/>.
	/// </summary>
	public static int GenerateGaussian(int mean,int deviation)
	{
		var gaussian = GenerateGaussian();

		return mean+Convert.ToInt32(gaussian*deviation);
	}

	/// <summary>
	/// Returns a random value from a normal distribution with <paramref name="mean"/> and <paramref name="deviation"/>.
	/// </summary>
	public static float GenerateGaussian(float mean,float deviation)
	{
		var gaussian = GenerateGaussian();

		return mean+Convert.ToSingle(gaussian*deviation);
	}

	/// <summary>
	/// Returns a random value from a normal distribution with <paramref name="mean"/> and <paramref name="deviation"/>.
	/// </summary>
	public static double GenerateGaussian(double mean,double deviation)
	{
		var gaussian = GenerateGaussian();

		return mean+gaussian*deviation;
	}

	/// <summary>
	/// Returns a standard normal sample (mean 0, standard deviation 1) using the Box-Muller transform.
	/// </summary>
	private static double GenerateGaussian()
	{
		double radiusSquared;
		double randomX;
		double randomY;

		do
		{
			randomX = s_randomizer.PickDouble(-1.0d,+1.0d);
			randomY = s_randomizer.PickDouble(-1.0d,+1.0d);

			radiusSquared = randomX*randomX+randomY*randomY;
		}
		while(radiusSquared >= 1.0d || radiusSquared <= 1e-15d);

		return randomX*Math.Sqrt(-2.0d*Math.Log(radiusSquared)/radiusSquared);
	}
	#endregion Gaussian

	/// <summary>
	/// Returns a uniformly random element from <paramref name="list"/>.
	/// </summary>
	public static TValue GetRandomValue<TValue>(IList<TValue> list)
	{
		_IsValidList(list);

		var idx = s_randomizer.PickInteger(0,list.Count-1);

		return list[idx];
	}

	/// <summary>
	/// Returns a random element from <paramref name="list"/> using <paramref name="weightedArray"/> as per-index weights.
	/// </summary>
	public static TValue GetWeightedRandomValue<TValue>(IList<TValue> list,float[] weightedArray)
	{
		_IsValidList(list);

		if(weightedArray == null || weightedArray.Length != list.Count)
		{
			throw new ArgumentException("Weighted array length must match list count.",nameof(weightedArray));
		}

		var idx = s_randomizer.PickWeightedInteger(weightedArray);

		return list[idx];
	}

	/// <summary>
	/// Yields <paramref name="cnt"/> random elements from <paramref name="list"/>.
	/// When <paramref name="allowDuplicate"/> is false, yields up to <paramref name="cnt"/> unique elements without replacement.
	/// </summary>
	public static IEnumerable<TValue> GetRandomValueGroup<TValue>(IList<TValue> list,int cnt,bool allowDuplicate = true)
	{
		_IsValidList(list);

		if(cnt < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(cnt),cnt,"Count must be zero or greater.");
		}

		if(allowDuplicate)
		{
			for(var i=0;i<cnt;i++)
			{
				yield return GetRandomValue(list);
			}
		}
		else
		{
			Randomize(list);

			var maxCnt = Math.Min(cnt,list.Count);

			for(var i=0;i<maxCnt;i++)
			{
				yield return list[i];
			}
		}
	}

	/// <summary>
	/// Shuffles <paramref name="list"/> in place using the Fisher-Yates algorithm.
	/// </summary>
	public static void Randomize<TValue>(IList<TValue> list)
	{
		_IsValidList(list);

		if (list.Count < 2) 
		{
			return; 
		}

		for(var i=list.Count-1;i>0;i--)
		{
			int idx0 = s_randomizer.PickInteger(0,i);
			int idx1 = i;

			(list[idx0],list[idx1]) = (list[idx1],list[idx0]);
		}
	}

	/// <summary>
	/// Picks a random element from <paramref name="list"/>, removes it, and returns true.
	/// When the list has one element, returns it without removing.
	/// </summary>
	public static bool RemoveRandomValue<TValue>(IList<TValue> list,out TValue val)
	{
		_IsValidList(list);

		var count = list.Count;

		if(count == 1)
		{
			val = list[0];

			return true;
		}

		val = GetRandomValue(list);

		list.Remove(val);

		return true;
	}

	#region String
	/// <summary>
	/// Generates a random lowercase alphabetic string of <paramref name="length"/> characters.
	/// </summary>
	public static string GenerateRandomString(int length,bool allowDuplicate = true)
	{
		var textList = new List<char>("abcdefghijklmnopqrstuvwxyz");
		var maxLength = Math.Max(textList.Count,length);
		var characterList = new List<char>(GetRandomValueGroup(textList,maxLength,allowDuplicate));

		Randomize(characterList);

		return string.Concat(characterList);
	}
	#endregion String

	/// <summary>
	/// Returns true with probability <paramref name="percent"/> in [0, 1].
	/// </summary>
	public static bool HitRate(float percent)
	{
		return s_randomizer.HitRate(percent);
	}

	/// <summary>
	/// Returns true when a random value in [0, 1] falls within [<paramref name="minVal"/>, <paramref name="maxVal"/>].
	/// </summary>
	public static bool HitRateInRange(float minVal,float maxVal)
	{
		return s_randomizer.HitRateInRange(minVal,maxVal);
	}
	
	/// <summary>
	/// Returns true with probability <paramref name="percent"/> in [0, 1].
	/// </summary>
	public static bool HitRate(double percent)
	{
		return s_randomizer.HitRate(percent);
	}

	/// <summary>
	/// Returns true when a random value in [0, 1] falls within [<paramref name="minVal"/>, <paramref name="maxVal"/>].
	/// </summary>
	public static bool HitRateInRange(double minVal,double maxVal)
	{
		return s_randomizer.HitRateInRange(minVal,maxVal);
	}

	/// <summary>
	/// Returns -1, 0, or +1. When <paramref name="includeZero"/> is false, returns only -1 or +1.
	/// </summary>
	public static int GetRandomSign(bool includeZero = true)
	{
		return includeZero ? s_randomizer.PickInteger(0,2)-1 : s_randomizer.PickDouble() < 0.5d ? -1 : +1;
	}

	private static bool _IsValidList<TValue>(IList<TValue> list)
	{
		if(list == null || list.Count == 0)
		{
			throw new ArgumentException("List is null or empty");
		}

		return true;
	}
}