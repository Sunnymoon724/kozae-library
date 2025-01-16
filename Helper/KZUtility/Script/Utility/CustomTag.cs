using System;
using System.Collections.Generic;

/// <summary>
/// Reference : https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types
/// </summary>

namespace KZLib.KZUtility
{
	public abstract class CustomTag : IComparable
	{
		private readonly string m_name = string.Empty;

		private static readonly Dictionary<string,CustomTag> m_cacheTagDict = new Dictionary<string,CustomTag>();
		private static readonly object m_syncRoot = new object();

		protected CustomTag(string name)
		{
			lock(m_syncRoot)
			{
				if(m_cacheTagDict.ContainsKey(name))
				{
					throw new ArgumentException($"The name {name} is already used.");
				}

				m_name = name;
				m_cacheTagDict[name] = this;
			}
		}

		public static IEnumerable<TTag> GetCustomTypeGroup<TTag>() where TTag : CustomTag
		{
			foreach(var data in m_cacheTagDict.Values)
			{
				yield return (TTag) data;
			}
		}

		public static bool IsDefined<TEnumeration>(string name) where TEnumeration : CustomTag
		{
			return TryParse<TEnumeration>(name,out _);
		}

		public static TTag Parse<TTag>(string name) where TTag : CustomTag
		{
			if(!TryParse<TTag>(name,out var tag))
			{
				throw new NullReferenceException($"{name} is not included in CustomType. [Type : {nameof(TTag)}]");
			}

			return tag!;
		}

		public static bool TryParse<TTag>(string name,out TTag? tag) where TTag : CustomTag
		{
			lock(m_syncRoot)
			{
				if(!m_cacheTagDict.TryGetValue(name,out var result))
				{
					tag = default;

					return false;
				}

				tag = (TTag?) result;

				return true;
			}
		}

		public override bool Equals(object other)
		{
			return other is CustomTag type && string.Equals(m_name,type.m_name,StringComparison.Ordinal);
		}

		public static bool operator ==(CustomTag left,CustomTag right)
		{
			if(ReferenceEquals(left,right))
			{
				return true;
			}

			if(left is null || right is null)
			{
				return false;
			}

			return left.Equals(right);
		}

		public static bool operator !=(CustomTag left,CustomTag right)
		{
			return !(left == right);
		}

		public override int GetHashCode() => m_name.GetHashCode();

		public override string ToString() => m_name;

		public int CompareTo(object other)
		{
			return other is CustomTag type ? m_name.CompareTo(type.m_name) : -1;
		}
	}
}