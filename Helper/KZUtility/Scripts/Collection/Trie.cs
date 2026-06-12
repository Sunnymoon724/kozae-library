using System;
using System.Collections.Generic;

namespace KZLib.Collections.Generic
{
	/// <summary>
	/// Thread-safe prefix tree for case-insensitive string lookup (Unicode-aware).
	/// </summary>
	/// <remarks>
	/// All lookups normalize input with <see cref="string.ToLowerInvariant"/>.
	/// Child edges are keyed by <see cref="char"/>, so English, Korean, digits, and symbols
	/// inserted via <see cref="Insert"/> are traversed the same way in prefix APIs.
	/// Text-scanning helpers can optionally skip characters that were never inserted.
	/// </remarks>
	public sealed class Trie
	{
		private const int c_initialBufferSize = 16;

		/// <summary>Single trie node with lazy child allocation.</summary>
		private class TrieNode
		{
			private readonly Dictionary<char,TrieNode> m_childDict = new();

			/// <summary>Whether a complete word ends at this node.</summary>
			public bool IsEndOfWord { get; set; }

			public bool HasChildren => m_childDict.Count > 0;

			/// <summary>Gets an existing child or creates and links a new one.</summary>
			public TrieNode GetOrCreate(char letter)
			{
				if(!m_childDict.TryGetValue(letter,out var node))
				{
					node = new TrieNode();

					m_childDict.Add(letter,node);
				}

				return node;
			}

			public bool TryGetNode(char letter,out TrieNode node) => m_childDict.TryGetValue(letter,out node!);

			public bool RemoveChild(char letter) => m_childDict.Remove(letter);

			/// <summary>Exposes child edges for traversal without copying the dictionary.</summary>
			public IEnumerable<KeyValuePair<char,TrieNode>> Children
			{
				get
				{
					foreach(var pair in m_childDict)
					{
						yield return pair;
					}
				}
			}
		}

		private readonly object m_syncRoot = new();

		private readonly TrieNode m_root = new();

		/// <summary>
		/// Inserts a normalized word when it is not already present.
		/// </summary>
		/// <returns><see langword="false"/> for blank input or duplicate words.</returns>
		public bool Insert(string word)
		{
			if(string.IsNullOrWhiteSpace(word))
			{
				return false;
			}

			var lower = word.ToLowerInvariant();

			lock(m_syncRoot)
			{
				var node = m_root;

				foreach(char letter in lower)
				{
					node = node.GetOrCreate(letter);
				}

				if(node.IsEndOfWord)
				{
					return false;
				}

				node.IsEndOfWord = true;

				return true;
			}
		}

		/// <summary>Whether the exact word exists in the trie.</summary>
		public bool Contains(string word)
		{
			if(string.IsNullOrWhiteSpace(word))
			{
				return false;
			}

			var lower = word.ToLowerInvariant();

			lock(m_syncRoot)
			{
				var node = m_root;

				foreach(char letter in lower)
				{
					if(!node.TryGetNode(letter,out node))
					{
						return false;
					}
				}

				return node.IsEndOfWord;
			}
		}

		/// <summary>Removes an exact word when present and prunes unused nodes.</summary>
		/// <returns><see langword="false"/> when the word is absent.</returns>
		public bool Remove(string word)
		{
			if(string.IsNullOrWhiteSpace(word))
			{
				return false;
			}

			var lower = word.ToLowerInvariant();

			lock(m_syncRoot)
			{
				return _RemoveRecursive(m_root,lower,0);
			}
		}

		/// <summary>
		/// Finds the length of the longest dictionary prefix of <paramref name="word"/> starting at <paramref name="start"/>.
		/// </summary>
		/// <param name="word">Text to scan.</param>
		/// <param name="start">Zero-based offset into <paramref name="word"/>.</param>
		/// <param name="onlyLetter">When true, stops at the first character for which <see cref="char.IsLetter"/> is false.</param>
		/// <returns>Matched prefix length, or 0 when no prefix matches.</returns>
		public int FindLongestPrefixLength(string word,int start = 0,bool onlyLetter = false)
		{
			if(string.IsNullOrWhiteSpace(word) || start >= word.Length)
			{
				return 0;
			}

			var lower = word.ToLowerInvariant();

			lock(m_syncRoot)
			{
				var node = m_root;
				var matched = 0;

				for(var i=start;i<lower.Length;i++)
				{
					var letter = lower[i];

					if(onlyLetter && !char.IsLetter(letter))
					{
						break;
					}

					if(!node.TryGetNode(letter,out node))
					{
						break;
					}

					if(node.IsEndOfWord)
					{
						matched = i-start+1;
					}
				}

				return matched;
			}
		}

		/// <summary>
		/// Returns character indices covered by the first dictionary word found in <paramref name="word"/>.
		/// </summary>
		/// <param name="word">Text to scan from every start offset.</param>
		/// <param name="onlyLetters">When true, skips source characters that fail the letter/digit filter.</param>
		/// <param name="allowDigit">When <paramref name="onlyLetters"/> is true, also allows digits.</param>
		public List<int> FindFirstWordIndices(string word,bool onlyLetters = true,bool allowDigit = false)
		{
			var resultList = new List<int>();

			if(string.IsNullOrWhiteSpace(word))
			{
				return resultList;
			}

			var lower = word.ToLowerInvariant();

			lock(m_syncRoot)
			{
				if(_TryFindWordRange(lower,onlyLetters,allowDigit,out var start,out var end))
				{
					for(var k=start;k<=end;k++)
					{
						resultList.Add(k);
					}
				}
			}

			return resultList;
		}

		/// <summary>
		/// Returns inclusive character ranges for every dictionary word found in <paramref name="word"/>.
		/// </summary>
		/// <param name="word">Text to scan.</param>
		/// <param name="onlyLetters">When true, skips source characters that fail the letter/digit filter.</param>
		/// <param name="allowDigit">When <paramref name="onlyLetters"/> is true, also allows digits.</param>
		public List<(int start,int end)> FindAllWordRanges(string word,bool onlyLetters = true,bool allowDigit = false)
		{
			var resultList = new List<(int start,int end)>();

			if(string.IsNullOrWhiteSpace(word))
			{
				return resultList;
			}

			var lower = word.ToLowerInvariant();

			lock(m_syncRoot)
			{
				for(var i=0;i<lower.Length;i++)
				{
					var node = m_root;

					for(var j=i;j<lower.Length;j++)
					{
						var letter = lower[j];

						if(!_IsScannableChar(letter,onlyLetters,allowDigit))
						{
							break;
						}

						if(!node.TryGetNode(letter,out node))
						{
							break;
						}

						if(node.IsEndOfWord)
						{
							resultList.Add((i,j));
						}
					}
				}
			}

			return resultList;
		}

		/// <summary>Whether any inserted word begins with <paramref name="prefix"/>.</summary>
		public bool StartsWith(string prefix)
		{
			if(string.IsNullOrWhiteSpace(prefix))
			{
				return false;
			}

			var lower = prefix.ToLowerInvariant();

			lock(m_syncRoot)
			{
				var node = m_root;

				foreach(var letter in lower)
				{
					if(!node.TryGetNode(letter,out node))
					{
						return false;
					}
				}

				return true;
			}
		}

		/// <summary>Returns all complete words that share the given prefix.</summary>
		public List<string> AutoComplete(string prefix)
		{
			var resultList = new List<string>();

			if(string.IsNullOrWhiteSpace(prefix))
			{
				return resultList;
			}

			var lower = prefix.ToLowerInvariant();

			lock(m_syncRoot)
			{
				var node = m_root;

				foreach(char letter in lower)
				{
					if(!node.TryGetNode(letter,out node))
					{
						return resultList;
					}
				}

				_DepthFirstSearch(node,lower,resultList);

				return resultList;
			}
		}

		/// <summary>Materializes every stored word via depth-first traversal.</summary>
		public List<string> ToList()
		{
			lock(m_syncRoot)
			{
				var resultList = new List<string>();

				_DepthFirstSearch(m_root,string.Empty,resultList);

				return resultList;
			}
		}

		private bool _TryFindWordRange(string lower,bool onlyLetters,bool allowDigit,out int start,out int end)
		{
			for(var i=0;i<lower.Length;i++)
			{
				var node = m_root;

				for(var j=i;j<lower.Length;j++)
				{
					var letter = lower[j];

					if(!_IsScannableChar(letter,onlyLetters,allowDigit))
					{
						break;
					}

					if(!node.TryGetNode(letter,out node))
					{
						break;
					}

					if(node.IsEndOfWord)
					{
						start = i;
						end = j;

						return true;
					}
				}
			}

			start = 0;
			end = -1;

			return false;
		}

		private bool _RemoveRecursive(TrieNode node,string word,int depth)
		{
			if(depth == word.Length)
			{
				if(!node.IsEndOfWord)
				{
					return false;
				}

				node.IsEndOfWord = false;

				return !node.HasChildren;
			}

			var letter = word[depth];

			if(!node.TryGetNode(letter,out var child))
			{
				return false;
			}

			if(_RemoveRecursive(child,word,depth+1))
			{
				node.RemoveChild(letter);
			}

			return false;
		}

		private static bool _IsScannableChar(char letter,bool onlyLetters,bool allowDigit)
		{
			if(!onlyLetters)
			{
				return true;
			}

			return allowDigit ? char.IsLetterOrDigit(letter) : char.IsLetter(letter);
		}

		/// <summary>
		/// Iterative DFS that reuses a char buffer and records words at end-of-word nodes.
		/// </summary>
		private void _DepthFirstSearch(TrieNode startNode,string prefix,List<string> result)
		{
			var bufferArray = new char[Math.Max(c_initialBufferSize,prefix.Length*2)];
			prefix.CopyTo(0,bufferArray,0,prefix.Length);

			var stack = new Stack<(TrieNode node,int length,char letter)>();

			stack.Push((startNode,prefix.Length,'\0'));

			while(stack.Count > 0)
			{
				var (node,length,letter) = stack.Pop();

				if(letter != '\0')
				{
					_EnsureCapacity(ref bufferArray,length);

					bufferArray[length-1] = letter;
				}

				if(node.IsEndOfWord)
				{
					result.Add(new string(bufferArray,0,length));
				}

				foreach(var pair in node.Children)
				{
					stack.Push((pair.Value,length+1,pair.Key));
				}
			}
		}

		/// <summary>Grows <paramref name="bufferArray"/> when the current path exceeds its length.</summary>
		private void _EnsureCapacity(ref char[] bufferArray,int required)
		{
			if(bufferArray.Length >= required)
			{
				return;
			}

			var newSize = bufferArray.Length*2;

			while(newSize < required)
			{
				newSize *= 2;
			}

			var newBuffer = new char[newSize];

			bufferArray.CopyTo(newBuffer,0);
			bufferArray = newBuffer;
		}
	}
}