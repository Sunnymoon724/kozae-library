using System;

namespace KZLib.Utilities
{
	/// <summary>
	/// Thread-safe disjoint-set structure for tracking connected groups of integer indices.
	/// </summary>
	/// <remarks>
	/// Indices range from <c>0</c> to <see cref="Count"/> - 1.
	/// <see cref="Union"/> merges two groups; <see cref="Connected"/> checks whether two indices share the same root.
	/// </remarks>
	public sealed class UnionFind
	{
		private readonly int[] m_parentArray = null!;
		private readonly byte[] m_rankArray = null!;
		private readonly object m_syncRoot = new();

		private int m_setCount = 0;

		/// <summary>Number of indexed elements.</summary>
		public int Count => m_parentArray.Length;

		/// <summary>Number of disjoint sets currently tracked.</summary>
		public int SetCount
		{
			get
			{
				lock(m_syncRoot)
				{
					return m_setCount;
				}
			}
		}

		/// <summary>Creates a partition of <paramref name="cnt"/> singleton sets.</summary>
		/// <param name="cnt">Element count; must be zero or greater.</param>
		public UnionFind(int cnt)
		{
			if(cnt < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(cnt));
			}

			m_parentArray = new int[cnt];
			m_rankArray = new byte[cnt];

			for(var i=0;i<cnt;i++)
			{
				m_parentArray[i] = i;
			}

			m_setCount = cnt;
		}

		/// <summary>Returns the representative root for <paramref name="idx"/> with path compression.</summary>
		/// <returns><see langword="false"/> when <paramref name="idx"/> is out of range.</returns>
		public bool TryFind(int idx,out int root)
		{
			lock(m_syncRoot)
			{
				return _TryFind(idx,out root);
			}
		}

		/// <summary>Whether <paramref name="first"/> and <paramref name="second"/> belong to the same set.</summary>
		/// <returns><see langword="false"/> when either index is out of range or the sets differ.</returns>
		public bool Connected(int first,int second)
		{
			lock(m_syncRoot)
			{
				if(!_TryFind(first,out var rootFirst) || !_TryFind(second,out var rootSecond))
				{
					return false;
				}

				return rootFirst == rootSecond;
			}
		}

		/// <summary>Merges the sets containing <paramref name="first"/> and <paramref name="second"/>.</summary>
		/// <returns><see langword="true"/> when the sets were different and merged.</returns>
		public bool Union(int first,int second)
		{
			lock(m_syncRoot)
			{
				if(!_TryFind(first,out var rootFirst) || !_TryFind(second,out var rootSecond))
				{
					return false;
				}

				if(rootFirst == rootSecond)
				{
					return false;
				}

				if(m_rankArray[rootFirst] < m_rankArray[rootSecond])
				{
					(rootFirst,rootSecond) = (rootSecond,rootFirst);
				}

				m_parentArray[rootSecond] = rootFirst;

				if(m_rankArray[rootFirst] == m_rankArray[rootSecond])
				{
					m_rankArray[rootFirst]++;
				}

				m_setCount--;

				return true;
			}
		}

		private bool _TryFind(int idx,out int root)
		{
			if(!_IsValidIndex(idx))
			{
				root = 0;

				return false;
			}

			root = _Find(idx);

			return true;
		}

		private int _Find(int idx)
		{
			var root = idx;

			while(m_parentArray[root] != root)
			{
				root = m_parentArray[root];
			}

			while(m_parentArray[idx] != root)
			{
				var next = m_parentArray[idx];

				m_parentArray[idx] = root;
				idx = next;
			}

			return root;
		}

		private bool _IsValidIndex(int idx)
		{
			return 0 <= idx && idx < m_parentArray.Length;
		}
	}
}