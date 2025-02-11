using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Reference : https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types
/// </summary>

namespace KZLib.KZData
{
	public abstract class CustomTag : IComparable,IComparable<CustomTag>,IEquatable<CustomTag>
	{
		private const BindingFlags c_tag_flag = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

		protected readonly string m_name = string.Empty;

		public string Name => m_name;

		protected CustomTag(string name)
		{
			m_name = name;
		}

		public static IEnumerable<TTag> GetCustomTagGroup<TTag>(bool isIncludeDerivedType) where TTag : CustomTag
		{
			var tagType = typeof(CustomTag);

			foreach(var tag in _GetCustomTagGroup<TTag>(tagType))
			{
				yield return tag;
			}

			if(isIncludeDerivedType)
			{
				foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach(var assemblyType in assembly.GetTypes())
					{
						if(tagType.IsAssignableFrom(assemblyType) && assemblyType != tagType)
						{
							foreach(var tag in _GetCustomTagGroup<TTag>(assemblyType))
							{
								yield return tag;
							}
						}
					}
				}
			}
		}

		private static IEnumerable<TTag> _GetCustomTagGroup<TTag>(Type tagType) where TTag : CustomTag
		{
			foreach(var fieldInfo in tagType.GetFields(c_tag_flag))
			{
				if(fieldInfo.GetValue(null) is TTag tag)
				{
					yield return tag;
				}
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

			return tag;
		}

		public static bool TryParse<TTag>(string name,out TTag resultTag) where TTag : CustomTag
		{
			foreach(var customTag in GetCustomTagGroup<TTag>(true))
			{
				if(string.Equals(customTag.m_name,name))
				{
					resultTag = customTag;

					return true;
				}
			}

			resultTag = null!;

			return false;
		}

		public override bool Equals(object other)
		{
			return other is CustomTag tag && Equals(tag);
		}

		public bool Equals(CustomTag tag)
		{
			return string.Equals(m_name,tag.m_name,StringComparison.Ordinal);
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
			return other is CustomTag tag ? CompareTo(tag) : throw new ArgumentException($"{other} is not a CustomTag");
		}

		public int CompareTo(CustomTag tag)
		{
			return m_name.CompareTo(tag.m_name);
		}
	}
}