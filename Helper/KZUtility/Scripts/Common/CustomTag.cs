using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Reference : https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types
/// </summary>

namespace KZLib.Utilities
{
	public abstract class CustomTag : IComparable,IComparable<CustomTag>,IEquatable<CustomTag>
	{
		private const BindingFlags c_tagFlag = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
		private static readonly Type s_baseType = typeof(CustomTag);

		protected readonly string m_name = string.Empty;

		public string Name => m_name;

		protected CustomTag(string name)
		{
			m_name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public static List<TTag> CollectCustomTagList<TTag>(bool isIncludeDerivedType) where TTag : CustomTag
		{
			var tagList = new List<TTag>();

			_CollectCustomTagGroup(tagList,s_baseType);

			if(isIncludeDerivedType)
			{
				foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach(var assemblyType in assembly.GetTypes())
					{
						if(assemblyType != s_baseType && s_baseType.IsAssignableFrom(assemblyType))
						{
							_CollectCustomTagGroup(tagList,assemblyType);
						}
					}
				}
			}

			return tagList;
		}

		private static void _CollectCustomTagGroup<TTag>(List<TTag> tagList,Type tagType) where TTag : CustomTag
		{
			foreach(var fieldInfo in tagType.GetFields(c_tagFlag))
			{
				if(fieldInfo.GetValue(null) is TTag tag)
				{
					tagList.Add(tag);
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
				throw new ArgumentException($"{name} is not included in CustomTag. [Type : {typeof(TTag).Name}]");
			}

			return tag;
		}

		public static bool TryParse<TTag>(string name,[NotNullWhen(true)] out TTag? resultTag) where TTag : CustomTag
		{
			foreach(var customTag in CollectCustomTagList<TTag>(true))
			{
				if(string.Equals(customTag.m_name,name,StringComparison.Ordinal))
				{
					resultTag = customTag;

					return true;
				}
			}

			resultTag = null;

			return false;
		}

		public override bool Equals(object? other)
		{
			return other is CustomTag tag && Equals(tag);
		}

		public bool Equals(CustomTag? other)
		{
			if(other is null)
			{
				return false;
			}

			return string.Equals(m_name,other.m_name,StringComparison.Ordinal);
		}

		public static bool operator ==(CustomTag? left,CustomTag? right)
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

		public static bool operator !=(CustomTag? left,CustomTag? right)
		{
			return !(left == right);
		}

		public override int GetHashCode() => m_name.GetHashCode();

		public override string ToString() => m_name;

		public int CompareTo(object? other)
		{
			if(other is CustomTag tag)
			{
				return CompareTo(tag);
			}

			throw new ArgumentException($"{other} is not a CustomTag");
		}

		public int CompareTo(CustomTag? tag)
		{
			if(tag is null)
			{
				return 1;
			}

			return string.Compare(m_name,tag.m_name,StringComparison.Ordinal);
		}
	}
}