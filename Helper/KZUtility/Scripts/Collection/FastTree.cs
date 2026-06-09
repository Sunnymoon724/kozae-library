using System;
using System.Collections.Generic;
using UnityEngine;

namespace KZLib.Collections.Generic
{
	public sealed class FastTree<TValue>
	{
		private readonly int m_unitNodeCount = 0;
		private readonly int m_depth = 0;
		private readonly Rect m_rectTree = Rect.zero;

		private readonly Dictionary<long,List<TValue>> m_valueListDict = new();
		private readonly object m_syncRoot = new();

		private readonly float m_halfWidth = 0.0f;
		private readonly float m_halfHeight = 0.0f;

		private const int c_unitNodeCount = 3;
		private const int c_nodeIndexBase = 10;
		private const int c_hexNibbleMask = 0x0F;

		public FastTree(int depth,Rect treeArea,int unitNodeCount = c_unitNodeCount)
		{
			if(depth < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(depth));
			}

			if(unitNodeCount <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(unitNodeCount));
			}

			if(unitNodeCount*unitNodeCount >= c_nodeIndexBase)
			{
				throw new ArgumentOutOfRangeException(nameof(unitNodeCount),$"Unit node count must produce fewer than {c_nodeIndexBase} child indices per level.");
			}

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
			if(string.IsNullOrEmpty(nodeID))
			{
				throw new ArgumentException("Node ID must not be null or empty.",nameof(nodeID));
			}

			var rect = m_rectTree;
			var maxIndex = m_unitNodeCount*m_unitNodeCount;

			for(var i=0;i<nodeID.Length;i++)
			{
				var index = nodeID[i] & c_hexNibbleMask;

				if(index < 1 || index > maxIndex)
				{
					throw new ArgumentException($"Invalid node ID character '{nodeID[i]}' at position {i}.",nameof(nodeID));
				}

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

			for(var i=0;i<m_unitNodeCount;i++)
			{
				for(var j=0;j<m_unitNodeCount;j++)
				{
					var childRect = new Rect(rect.x+width*i,rect.y+height*j,width,height);

					if(childRect.Contains(point))
					{
						var index = parentIndex*c_nodeIndexBase+j*m_unitNodeCount+i+1;

						return _FindNodeIndexRecursive(depth+1,index,point,childRect);
					}
				}
			}

			return parentIndex;
		}


		private Rect _FindNodeRectRecursive(int depth,Vector2 point,Rect rect)
		{
			if(depth > m_depth)
			{
				return rect;
			}

			var width = rect.width/m_unitNodeCount;
			var height = rect.height/m_unitNodeCount;

			for(var i=0;i<m_unitNodeCount;i++)
			{
				for(var j=0;j<m_unitNodeCount;j++)
				{
					var childRect = new Rect(rect.x+width*i,rect.y+height*j,width,height);

					if(childRect.Contains(point))
					{
						return _FindNodeRectRecursive(depth+1,point,childRect);
					}
				}
			}

			return rect;
		}

		private void _FindNodesIndexInBoundRecursive(int depth,long parentIndex,Rect srcRect,Rect dstRect,List<long> resultList)
		{
			if( depth > m_depth)
			{
				resultList.Add(parentIndex);

				return;
			}

			var width = srcRect.width/m_unitNodeCount;
			var height = srcRect.height/m_unitNodeCount;

			for(var i=0;i<m_unitNodeCount;i++)
			{
				for(var j=0;j<m_unitNodeCount;j++)
				{
					var childRect = new Rect(srcRect.x+width*i,srcRect.y+height*j,width,height);

					if(childRect.Overlaps(dstRect))
					{
						var index = parentIndex*c_nodeIndexBase+j*m_unitNodeCount+i+1;

						_FindNodesIndexInBoundRecursive(depth+1,index,childRect,dstRect,resultList);
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

			for(var i=0;i<m_unitNodeCount;i++)
			{
				for(var j=0;j<m_unitNodeCount;j++)
				{
					var childRect = new Rect(srcRect.x+width*i,srcRect.y+height*j,width,height);

					if(childRect.Overlaps(dstRect))
					{
						var index = parentIndex*c_nodeIndexBase+j*m_unitNodeCount+i+1;

						_FindNodesRectRecursive(depth+1,index,childRect,dstRect,resultList);
					}
				}
			}
		}

		public long UpdateValue(long oldIndex,TValue value,Vector2 point)
		{
			lock(m_syncRoot)
			{
				var newIndex = FindNodeIndex(point);

				if(newIndex == oldIndex)
				{
					return newIndex;
				}

				_RemoveValueUnsafe(oldIndex,value);
				_AddValueUnsafe(newIndex,value);

				return newIndex;
			}
		}

		public long AddValue(TValue value,Vector2 point)
		{
			lock(m_syncRoot)
			{
				var index = FindNodeIndex(point);

				_AddValueUnsafe(index,value);

				return index;
			}
		}

		private void _AddValueUnsafe(long key,TValue value)
		{
			if(!m_valueListDict.TryGetValue(key,out var valueList))
			{
				valueList = new List<TValue>();

				m_valueListDict.Add(key,valueList);
			}

			valueList.Add(value);
		}

		public void RemoveValue(TValue value,Vector2 point)
		{
			lock(m_syncRoot)
			{
				var index = FindNodeIndex(point);

				_RemoveValueUnsafe(index,value);
			}
		}

		private void _RemoveValueUnsafe(long key,TValue value)
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

			lock(m_syncRoot)
			{
				for(var i=0;i<indexList.Count;i++)
				{
					var index = indexList[i];

					if(m_valueListDict.TryGetValue(index,out var valueList))
					{
						resultList.AddRange(valueList);
					}
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

		public TValue[] ToArray()
		{
			lock(m_syncRoot)
			{
				var valueList = new List<TValue>();

				foreach(var pair in m_valueListDict)
				{
					if(pair.Value != null)
					{
						valueList.AddRange(pair.Value);
					}
				}

				return valueList.ToArray();
			}
		}
	}
}