using System;
using System.Collections.Generic;

namespace KZLib.Collections.Generic
{
	public sealed class Trie
	{
		private class TrieNode
		{
			private readonly Dictionary<char,TrieNode> m_childDict = new();
			public bool IsEndOfWord { get; set; }

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
					// Already exists
					return false;
				}

				node.IsEndOfWord = true;

				return true;
			}
		}

		public int Search(string word,int start = 0,bool onlyLetter = true)
		{
			if(string.IsNullOrWhiteSpace(word) || start >= word.Length )
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
		
		public List<int> ExtractWordIndexList(string word,bool allowDigit = false)
		{
            var resultList = new List<int>();

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

						if(allowDigit)
						{
							if(!char.IsLetterOrDigit(letter))
							{
								break;
							}
						}
						else
						{
							if(!char.IsLetter(letter))
							{
								break;
							}
						}

						if(!node.TryGetNode(letter,out node))
						{
							break;
						}

						if(node.IsEndOfWord)
						{
							for(var k=i;k<=j;k++)
							{
								resultList.Add(k);
							}

							return resultList;
						}
					}
				}
			}

			return resultList;
		}

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
					if(!char.IsLetter(letter) || !node.TryGetNode(letter,out node))
					{
						return false;
					}
				}

				return true;
			}
		}
		
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
					if(!char.IsLetter(letter) || !node.TryGetNode(letter,out node))
					{
						return resultList;
					}
				}

				_DepthFirstSearch(node,lower,resultList);

				return resultList;
			}
		}

		public List<string> ToList()
		{
			lock(m_syncRoot)
			{
				var resultList = new List<string>();

				_DepthFirstSearch(m_root,string.Empty,resultList);

				return resultList;
			}
		}

		private void _DepthFirstSearch(TrieNode startNode,string prefix,List<string> result)
		{
			var bufferArray = new char[Math.Max(16,prefix.Length*2)];
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