
namespace System.Collections.Generic
{
	public sealed class Trie
	{
		private class TrieNode
		{
			private readonly Dictionary<char,TrieNode> m_childDict;

			public bool IsEndOfWord { get; set; }

			public TrieNode()
			{
				m_childDict = new Dictionary<char,TrieNode>();
				IsEndOfWord = false;
			}

			public TrieNode GetOrCreateTrieNode(char letter)
			{
				if(!m_childDict.TryGetValue(letter,out var node))
				{
					node = new TrieNode();

					m_childDict.Add(letter,node);
				}

				return node;
			}
			
			public bool TryGetTrieNode(char letter, out TrieNode node)
			{
				if(m_childDict.TryGetValue(letter,out var trieNode) && trieNode != null)
				{
					node = trieNode;

					return true;
				}

				node = null!;
	
				return false;
			}

			public IEnumerable<KeyValuePair<char,TrieNode>> GetChildren()
            {
                return m_childDict;
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
			
			lock(m_syncRoot)
			{
				var node = m_root;

				foreach(char letter in word)
				{
					node = node.GetOrCreateTrieNode(letter);
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

			var node = m_root;
			var matchLength = 0;
			var length = 0;

			lock(m_syncRoot)
			{
				for( int i = start; i < word.Length; i++ )
				{
					char letter = word[ i ];

					if( onlyLetter && !char.IsLetter( letter ) )
					{
						continue; // Skip non-letters
					}

					if(!node.TryGetTrieNode(letter,out node))
					{
						break;
					}

					length++;

					if(node.IsEndOfWord)
					{
						matchLength = length;
					}
				}

				return matchLength;
			}
		}
		
		public List<int> ExtractWordIndexList(string input,bool skipDigit)
		{
            var indexList = new List<int>();

			if(!string.IsNullOrWhiteSpace(input))
			{
				var lower = input.ToLowerInvariant();
				var index = 0;

				while(index < lower.Length)
				{
					var node = m_root;
					var tempIndex = index;
                    var tempIndexList = new List<int>();

					while(tempIndex < lower.Length)
					{
						var letter = lower[tempIndex];

						if(skipDigit)
						{
							if(!char.IsLetter(letter))
							{
								tempIndex++;

								continue;
							}
						}
						else
						{
							if(!char.IsLetterOrDigit(letter))
							{
								tempIndex++;

								continue;
							}
						}

						if(!node.TryGetTrieNode(letter,out node))
						{
							break;
						}

						tempIndexList.Add(tempIndex);
						tempIndex++;

						if(node.IsEndOfWord)
						{
							indexList.AddRange(tempIndexList);

							break;
						}
					}

					index++;
				}
			}

			return indexList;
		}

		public bool StartsWith(string prefix)
		{
			if(string.IsNullOrWhiteSpace(prefix))
			{
				return false;
			}

			lock(m_syncRoot)
			{
				var node = m_root;

				foreach(char letter in prefix)
				{
					if(!node.TryGetTrieNode(letter,out node))
					{
						return false;
					}
				}

				return true;
			}
		}
		
		public List<string> AutoComplete(string prefix)
		{
			if(string.IsNullOrWhiteSpace(prefix))
			{
				return new List<string>();
			}

			lock(m_syncRoot)
			{
				var resultList = new List<string>();
				var node = m_root;

				foreach(char letter in prefix)
				{
					if(!node.TryGetTrieNode(letter,out node))
					{
						return resultList;
					}
				}

				_DepthFirstSearch(node,prefix,resultList);

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
			var itemStack = new Stack<StackItem>();

			itemStack.Push(new StackItem(startNode,prefix));

			while(itemStack.Count > 0)
			{
				var item = itemStack.Pop();

				if(item.Node.IsEndOfWord)
				{
					result.Add(item.Current);
				}

				foreach(var pair in item.Node.GetChildren())
				{
					itemStack.Push(new StackItem(pair.Value,item.Current+pair.Key));
				}
			}
		}

		private record StackItem(TrieNode Node,string Current);
	}
}