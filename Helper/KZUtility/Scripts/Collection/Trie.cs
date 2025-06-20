
namespace System.Collections.Generic
{
	public class Trie
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
				var result = m_childDict.TryGetValue(letter,out node);

				return result;
			}

			public IEnumerable<KeyValuePair<char,TrieNode>> GetChildren()
            {
                return m_childDict;
            }
		}
		
		private readonly object m_syncRoot = new object();
		
		private readonly TrieNode m_root = new TrieNode();

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

		public bool Search(string word)
		{
			if(string.IsNullOrWhiteSpace(word))
			{
				return false;
			}

			var node = m_root;

			lock(m_syncRoot)
			{
				foreach(char letter in word)
				{
					if(!node.TryGetTrieNode(letter,out node))
					{
						return false;
					}
				}

				return node.IsEndOfWord;
			}
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

				DepthFirstSearch(node,prefix,resultList);

				return resultList;
			}
		}
		
		public List<string> ToList()
		{
			lock(m_syncRoot)
			{
				var resultList = new List<string>();

				DepthFirstSearch(m_root,"",resultList);

				return resultList;
			}
		}
		
		private void DepthFirstSearch(TrieNode startNode,string prefix,List<string> result)
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

		private class StackItem
		{
			public TrieNode Node { get; }
			public string Current { get; }

			public StackItem(TrieNode node,string current)
			{
				Node = node;
				Current = current;
			}
		}
	}
}