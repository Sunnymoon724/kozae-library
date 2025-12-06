using System;
using System.Collections.Generic;

namespace KZLib.KZUtility
{
	public static class RandomUtility
	{
		private static readonly Randomizer s_randomizer = new();

		#region Gaussian
		public static int GenerateGaussian(int mean,int deviation)
		{
			var gaussian = GenerateGaussian();

			return mean+Convert.ToInt32(gaussian*deviation);
		}

		public static float GenerateGaussian(float mean,float deviation)
		{
			var gaussian = GenerateGaussian();

			return mean+Convert.ToSingle(gaussian*deviation);
		}

		public static double GenerateGaussian(double mean,double deviation)
		{
			var gaussian = GenerateGaussian();

			return mean+gaussian*deviation;
		}

		private static double GenerateGaussian()
		{
			double radiusSquared, randomX, randomY;

			do
			{
				randomX = s_randomizer.PickDouble(-1.0d,+1.0d);
				randomY = s_randomizer.PickDouble(-1.0d,+1.0d);

				radiusSquared = randomX * randomX + randomY * randomY;
			}
			while (radiusSquared >= 1.0f || Math.Abs(radiusSquared) <= 1e-15d);

			return randomX*Math.Sqrt(-2.0f*Math.Log(radiusSquared)/radiusSquared);
		}
		#endregion Gaussian

		public static TValue GetRandomValue<TValue>(IList<TValue> list)
		{
			_IsValidList(list);

			var index = s_randomizer.PickInteger(0,list.Count-1);

			return list[index];
		}

		public static TValue GetWeightedRandomValue<TValue>(IList<TValue> list,float[] weightedArray)
		{
			_IsValidList(list);

			var index = s_randomizer.PickWeightedInteger(weightedArray);

			return list[index];
		}

		public static IEnumerable<TValue> GetRandomValueGroup<TValue>(IList<TValue> list,int count,bool allowDuplicate = true)
		{
			_IsValidList(list);

			if(allowDuplicate)
			{
				for(var i=0;i<count;i++)
				{
					yield return GetRandomValue(list);
				}
			}
			else
			{
				Randomize(list);

				var maxCount = Math.Max(count,list.Count);

				for(var i=0;i<maxCount;i++)
				{
					yield return list[i];
				}
			}
		}

		public static void Randomize<TValue>(IList<TValue> list)
		{
			_IsValidList(list);

			if (list.Count < 2) 
			{
				return; 
			}

			for(var i=list.Count-1;i>0;i--)
			{
				int index0 = s_randomizer.PickInteger(0,i);
				int index1 = i;

				(list[index0],list[index1]) = (list[index1],list[index0]);
			}
		}

		public static bool RemoveRandomValue<TValue>(IList<TValue> list,out TValue value)
		{
			_IsValidList(list);

			var count = list.Count;

			if(count == 1)
			{
				value = list[0];

				return true;
			}

			value = GetRandomValue(list);

			list.Remove(value);

			return true;
		}

		#region String
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
		/// Check value in [0,1].
		/// </summary>
		public static bool HitRate(float percent)
		{
			return s_randomizer.PickSingle() <= percent;
		}

		/// <summary>
		/// Check value in [minValue,maxValue].
		/// </summary>
		public static bool HitRateInRange(float minValue,float maxValue)
		{
			var value = s_randomizer.PickSingle();

			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			return minValue <= value && value <= maxValue;
		}
		
		/// <summary>
		/// Check value in [0,1].
		/// </summary>
		public static bool HitRate(double percent)
		{
			return s_randomizer.PickDouble() <= percent;
		}

		/// <summary>
		/// Check value in [minValue,maxValue].
		/// </summary>
		public static bool HitRateInRange(double minValue,double maxValue)
		{
			var value = s_randomizer.PickDouble();

			(minValue,maxValue) = _NormalizeRange(minValue,maxValue);

			return minValue <= value && value <= maxValue;
		}

		public static int GetRandomSign(bool includeZero = true)
		{
			return includeZero ? s_randomizer.PickInteger(0,2)-1 : s_randomizer.PickDouble() < 0.5d ? -1 : 1;
		}

		private static (TValue min,TValue max) _NormalizeRange<TValue>(TValue minValue,TValue maxValue) where TValue : IComparable<TValue>
		{
			return minValue.CompareTo(maxValue) > 0 ? (maxValue,minValue) : (minValue,maxValue);
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
}