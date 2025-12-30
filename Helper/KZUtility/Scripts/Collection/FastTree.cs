using UnityEngine;

namespace System.Collections.Generic
{
	public sealed class FastTree<TValue>
	{
		private readonly int m_unitNodeCount = 0;
		private readonly int m_depth = 0;
		private readonly Rect m_rectTree = Rect.zero;

		private readonly Dictionary<long,List<TValue>> m_valueListDict = new();

		private readonly float m_halfWidth = 0.0f;
		private readonly float m_halfHeight = 0.0f;

		private Rect m_tempRect = Rect.zero;

		public FastTree(int depth,Rect treeArea,int unitNodeCount = 3)
		{
			m_depth = depth;

			m_unitNodeCount = unitNodeCount;
			m_rectTree = treeArea;

			m_halfWidth = -m_rectTree.x;
			m_halfHeight = -m_rectTree.y;
		}

		public long FindNodeIndex(Vector2 point)
		{
			point = Convert2TreeCoordinate(point);

			return _FindNodeIndexRecursive(1,0,point,m_rectTree);
		}

		public Rect FindNodeRect(Vector2 point)
		{
			point = Convert2TreeCoordinate(point);

			var rect = _FindNodeRectRecursive(1,point,m_rectTree);

			rect.x -= m_halfWidth;
			rect.y -= m_halfHeight;

			return rect;
		}

		public Rect FindNodeRect(string nodeID)
		{
			var rect = m_rectTree;

			for(var i=0;i<nodeID.Length;i++)
			{
				var depth = i;

				var index = nodeID[depth] & 15;

				var width = rect.width/m_unitNodeCount;
				var height = rect.height/m_unitNodeCount;

				var column = (index-1)%m_unitNodeCount;
				var row = (index-1)/m_unitNodeCount;

				rect.x += width*column;
				rect.y += height*row;

				rect.width = width;
				rect.height = height;
			}

			rect.x -= m_halfWidth;
			rect.y -= m_halfHeight;

			return rect;
		}

		public Vector2 GetNodeSize()
		{
			var width = m_rectTree.width;
			var height = m_rectTree.height;

			for(var i=0;i<m_depth;i++)
			{
				width /= m_unitNodeCount;
				height /= m_unitNodeCount;
			}

			return new Vector2(width,height);
		}

		private long _FindNodeIndexRecursive(int depth,long parentIndex,Vector2 point,Rect rect)
		{
			if(depth > m_depth)
			{
				return parentIndex;
			}

			var width = rect.width/m_unitNodeCount;
			var height = rect.height/m_unitNodeCount;

			m_tempRect.width = width;
			m_tempRect.height = height;

			for(var i=0;i<m_unitNodeCount;i++)
			{
				for(var j=0;j<m_unitNodeCount;j++)
				{
					m_tempRect.x = rect.x+width*i;
					m_tempRect.y = rect.y+height*j;

					if(m_tempRect.Contains(point))
					{
						var index = parentIndex*10+j*m_unitNodeCount+i+1;

						return _FindNodeIndexRecursive(depth+1,index,point,m_tempRect);
					}
				}
			}

			return parentIndex;
		}


		private Rect _FindNodeRectRecursive(int _depth,Vector2 point,Rect rect)
		{
			if(_depth > m_depth)
			{
				return rect;
			}

			var width = rect.width/m_unitNodeCount;
			var height = rect.height/m_unitNodeCount;

			m_tempRect.width = width;
			m_tempRect.height = height;

			for(var i=0;i<m_unitNodeCount;i++)
			{
				for(var j=0;j<m_unitNodeCount;j++)
				{
					m_tempRect.x = rect.x+width*i;
					m_tempRect.y = rect.y+height*j;

					if(m_tempRect.Contains(point))
					{
						return _FindNodeRectRecursive(_depth+1,point,m_tempRect);
					}
				}
			}

			return rect;
		}

		private void _FindNodesIndexInBoundRecursive(int _depth,long _parentIndex,Rect srcRect,Rect dstRect,List<long> resultList)
		{
			if( _depth > m_depth)
			{
				resultList.Add(_parentIndex);

				return;
			}

			var width = srcRect.width/m_unitNodeCount;
			var height = srcRect.height/m_unitNodeCount;

			m_tempRect.width = width;
			m_tempRect.height = height;

			for(var i=0;i<m_unitNodeCount;i++)
			{
				for(var j=0;j<m_unitNodeCount;j++)
				{
					m_tempRect.x = srcRect.x+width*i;
					m_tempRect.y = srcRect.y+height*j;

					if(m_tempRect.Overlaps(dstRect))
					{
						var index = _parentIndex*10+j*m_unitNodeCount+i+1;

						_FindNodesIndexInBoundRecursive(_depth+1,index,m_tempRect,dstRect,resultList);
					}
				}
			}
		}

		public List<Rect> FindNodeRectInBound(Rect rect)
		{
			rect = ConvertRect2TreeCoordinate(rect);

			var rectList = new List<Rect>();

			_FindNodesRectRecursive(1,0,m_rectTree,rect,rectList);

			return rectList;
		}

		private void _FindNodesRectRecursive(int depth,long parentIndex,Rect srcRect,Rect dstRect,List<Rect> resultList)
		{
			if(depth > m_depth)
			{
				srcRect.x -= m_halfWidth;
				srcRect.y -= m_halfHeight;

				resultList.Add(srcRect);

				return;
			}

			var width = srcRect.width/m_unitNodeCount;
			var height = srcRect.height/m_unitNodeCount;

			m_tempRect.width = width;
			m_tempRect.height = height;

			for(var i=0;i<m_unitNodeCount;i++)
			{
				for(var j=0;j<m_unitNodeCount;j++)
				{
					m_tempRect.x = srcRect.x+width*i;
					m_tempRect.y = srcRect.y+height*j;

					if(m_tempRect.Overlaps(dstRect))
					{
						var index = parentIndex*10+j*m_unitNodeCount+i+1;

						_FindNodesRectRecursive(depth+1,index,m_tempRect,dstRect,resultList);
					}
				}
			}
		}

		public long UpdateValue(long oldIndex,TValue value,Vector2 point)
		{
			var newIndex = FindNodeIndex(point);

			if(newIndex == oldIndex)
			{
				return newIndex;
			}

			_RemoveValue(oldIndex,value);
			_AddValue(newIndex,value);

			return newIndex;
		}

		public long AddValue(TValue value,Vector2 point)
		{
			var index = FindNodeIndex(point);

			_AddValue(index,value);

			return index;
		}

		private long _AddValue(long key,TValue value)
		{
			if(!m_valueListDict.TryGetValue(key,out var valueList))
			{
				valueList = new List<TValue>();

				m_valueListDict.Add(key,valueList);
			}

			valueList.Add(value);

			return key;
		}

		public void RemoveValue(TValue value,Vector2 point)
		{
			var index = FindNodeIndex(point);

			_RemoveValue(index,value);
		}

		private void _RemoveValue(long key,TValue value)
		{
			if(m_valueListDict.TryGetValue(key,out List<TValue> valueList))
			{
				valueList.Remove(value);

				if(valueList.Count == 0)
				{
					m_valueListDict.Remove(key);
				}
			}
		}

		public List<long> FindNodeInBound(Rect rect)
		{
			var indexList = new List<long>();

			rect = ConvertRect2TreeCoordinate(rect);

			_FindNodesIndexInBoundRecursive(1,0,m_rectTree,rect,indexList);

			return indexList;
		}

		public List<TValue> CreateValueInBound(Rect rect)
		{
			var resultList = new List<TValue>();
			var indexList = FindNodeInBound(rect);

			for(var i=0;i<indexList.Count;i++)
			{
				var index = indexList[i];

				if(m_valueListDict.TryGetValue(index, out var valueList))
				{
					resultList.AddRange(valueList);
				}
			}

			return resultList;
		}

		public Vector2 Convert2TreeCoordinate(Vector2 point)
		{
			point.x += m_halfWidth;
			point.y += m_halfHeight;

			return point;
		}

		public Vector2 Convert2GeoCoordinate(Vector2 point)
		{
			point.x -= m_halfWidth;
			point.y -= m_halfHeight;

			return point;
		}

		public Rect ConvertRect2TreeCoordinate(Rect rect)
		{
			rect.x += m_halfWidth;
			rect.y += m_halfHeight;

			return rect;
		}
	}
}