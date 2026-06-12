using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KZLib.Utilities
{
	/// <summary>
	/// Smart-enum base class: named tag instances are exposed as public static fields on derived types
	/// instead of C# <see langword="enum"/> values.
	/// Derived projects can add new tags by declaring subclasses and static fields without modifying the library.
	/// </summary>
	/// <example>
	/// <code>
	/// // Library
	/// public class CommonUINameTag : CustomTag
	/// {
	///     public static readonly CommonUINameTag LoadingPanel = new(nameof(LoadingPanel));
	///     public CommonUINameTag(string name) : base(name) { }
	/// }
	///
	/// // Consumer project
	/// public class ExtendedUINameTag : CommonUINameTag
	/// {
	///     public static readonly ExtendedUINameTag ShopPanel = new(nameof(ShopPanel));
	///     public ExtendedUINameTag(string name) : base(name) { }
	/// }
	///
	/// CustomTag.TryParse&lt;CommonUINameTag&gt;("ShopPanel", out var tag);
	/// </code>
	/// </example>
	/// <remarks>
	/// Reference: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types
	/// </remarks>
	public abstract class CustomTag : IComparable,IComparable<CustomTag>,IEquatable<CustomTag>
	{
		private const BindingFlags c_tagFlag = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
		private static readonly object s_cacheSync = new();
		private static readonly Dictionary<Type,object> s_tagListCache = new();
		private static readonly Dictionary<Type,object> s_tagLookupCache = new();

		protected readonly string m_name = string.Empty;

		/// <summary>Display name of this tag instance.</summary>
		public string Name => m_name;

		/// <summary>
		/// Creates a tag with the given name.
		/// </summary>
		protected CustomTag(string name)
		{
			m_name = name ?? throw new ArgumentNullException(nameof(name));
		}

		/// <summary>
		/// Collects public static tag fields declared on <typeparamref name="TTag"/> and its subclasses in loaded assemblies.
		/// Results are cached per tag type.
		/// </summary>
		public static List<TTag> CollectCustomTagList<TTag>() where TTag : CustomTag
		{
			return _GetOrCreateTagList<TTag>();
		}

		private static List<TTag> _GetOrCreateTagList<TTag>() where TTag : CustomTag
		{
			lock(s_cacheSync)
			{
				if(s_tagListCache.TryGetValue(typeof(TTag),out var cachedList))
				{
					return (List<TTag>)cachedList;
				}

				var tagList = _CollectCustomTagListCore<TTag>();

				s_tagListCache[typeof(TTag)] = tagList;

				return tagList;
			}
		}

		private static List<TTag> _CollectCustomTagListCore<TTag>() where TTag : CustomTag
		{
			var tagList = new List<TTag>();

			_CollectCustomTagGroup(tagList,typeof(TTag));

			foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach(var assemblyType in assembly.GetTypes())
				{
					if(assemblyType == typeof(TTag) || !typeof(TTag).IsAssignableFrom(assemblyType))
					{
						continue;
					}

					_CollectCustomTagGroup(tagList,assemblyType);
				}
			}

			return tagList;
		}

		/// <summary>
		/// Appends public static fields declared on <paramref name="tagType"/> whose values are <typeparamref name="TTag"/>.
		/// </summary>
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

		private static Dictionary<string,TTag> _GetOrCreateTagLookup<TTag>() where TTag : CustomTag
		{
			lock(s_cacheSync)
			{
				if(s_tagLookupCache.TryGetValue(typeof(TTag),out var cachedLookup))
				{
					return (Dictionary<string,TTag>)cachedLookup;
				}

				var tagList = _GetOrCreateTagList<TTag>();

				var tagLookup = new Dictionary<string,TTag>(StringComparer.Ordinal);

				foreach(var tag in tagList)
				{
					tagLookup[tag.m_name] = tag;
				}

				s_tagLookupCache[typeof(TTag)] = tagLookup;

				return tagLookup;
			}
		}

		/// <summary>
		/// Returns whether a tag with the given name exists for <typeparamref name="TEnumeration"/>.
		/// Includes tags declared on subclasses of <typeparamref name="TEnumeration"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
		public static bool IsDefined<TEnumeration>(string name) where TEnumeration : CustomTag
		{
			return TryParse<TEnumeration>(name,out _);
		}

		/// <summary>
		/// Resolves a tag by name. Throws when the name is not found.
		/// Includes tags declared on subclasses of <typeparamref name="TTag"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="name"/> does not match any tag.</exception>
		public static TTag Parse<TTag>(string name) where TTag : CustomTag
		{
			if(!TryParse<TTag>(name,out var tag))
			{
				throw new ArgumentException($"{name} is not included in CustomTag. [Type : {typeof(TTag).Name}]");
			}

			return tag;
		}

		/// <summary>
		/// Resolves a tag by name. Performs a case-sensitive ordinal name comparison.
		/// Includes tags declared on subclasses of <typeparamref name="TTag"/> in loaded assemblies.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
		public static bool TryParse<TTag>(string name,[NotNullWhen(true)] out TTag? resultTag) where TTag : CustomTag
		{
			if(name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if(_GetOrCreateTagLookup<TTag>().TryGetValue(name,out resultTag))
			{
				return true;
			}

			resultTag = null;

			return false;
		}

		public override bool Equals(object? other)
		{
			return other is CustomTag tag && Equals(tag);
		}

		/// <summary>
		/// Tags are equal when their runtime type and name both match.
		/// </summary>
		public bool Equals(CustomTag? other)
		{
			if(other is null)
			{
				return false;
			}

			if(GetType() != other.GetType())
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

		public override int GetHashCode() => HashCode.Combine(GetType(),m_name);

		public override string ToString() => m_name;

		public int CompareTo(object? other)
		{
			if(other is CustomTag tag)
			{
				return CompareTo(tag);
			}

			throw new ArgumentException($"{other} is not a CustomTag");
		}

		/// <summary>
		/// Compares tags by runtime type, then by name. Null is treated as less than any tag.
		/// </summary>
		public int CompareTo(CustomTag? tag)
		{
			if(tag is null)
			{
				return 1;
			}

			var typeCompare = string.Compare(GetType().FullName,tag.GetType().FullName,StringComparison.Ordinal);

			if(typeCompare != 0)
			{
				return typeCompare;
			}

			return string.Compare(m_name,tag.m_name,StringComparison.Ordinal);
		}
	}
}
