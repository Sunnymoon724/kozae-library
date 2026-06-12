using System;
using System.Collections.Generic;
using UnityEngine;

namespace KZLib.Collections.Generic
{
	/// <summary>
	/// Uniform grid spatial index for 2D points with hierarchical node IDs.
	/// </summary>
	/// <typeparam name="TValue">Payload stored per leaf cell.</typeparam>
	/// <remarks>
	/// The tree subdivides a fixed <see cref="Rect"/> into <c>unitNodeCount × unitNodeCount</c>
	/// children per level down to <paramref name="depth"/>. Node indices are encoded in base
	/// <see cref="c_nodeIndexBase"/> so each tree level appends one digit to the path.
	/// Coordinates are shifted into tree space via <see cref="Convert2TreeCoordinate"/>.
	/// Points outside <see cref="Rect"/> throw or return <see langword="false"/> from Try APIs.
	/// When <c>depth == 0</c>, the entire area is a single leaf with node index <see cref="c_rootLeafIndex"/>.
	/// </remarks>
	public sealed class FastTree<TValue>
	{
		private readonly int m_unitNodeCount = 0;
		private readonly int m_depth = 0;
		private readonly Rect m_rectTree = Rect.zero;

		private readonly Dictionary<long,List<TValue>> m_valueListDict = new();
		private readonly object m_syncRoot = new();

		/// <summary>World-to-tree X offset (<c>-treeArea.x</c>).</summary>
		private readonly float m_treeOffsetX = 0.0f;

		/// <summary>World-to-tree Y offset (<c>-treeArea.y</c>).</summary>
		private readonly float m_treeOffsetY = 0.0f;

		/// <summary>Default subdivision factor (3×3 children per node).</summary>
		private const int c_unitNodeCount = 3;

		/// <summary>Radix for packing child indices into a <see langword="long"/> node ID.</summary>
		private const int c_nodeIndexBase = 10;

		/// <summary>Masks a character to its low hex nibble when decoding node ID strings.</summary>
		private const int c_hexNibbleMask = 0x0F;

		/// <summary>Node index used when <c>depth == 0</c> (single leaf covering the whole tree).</summary>
		private const long c_rootLeafIndex = 1;

		/// <summary>
		/// Creates a spatial tree over <paramref name="treeArea"/> with the given subdivision depth.
		/// </summary>
		/// <param name="depth">Number of subdivision levels below the root (0 = root only).</param>
		/// <param name="treeArea">World-space bounds; width and height must be positive.</param>
		/// <param name="unitNodeCount">Children per axis at each level; must be small enough for base-10 digit encoding.</param>
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

			if(treeArea.width <= 0.0f || treeArea.height <= 0.0f)
			{
				throw new ArgumentOutOfRangeException(nameof(treeArea),"Tree area width and height must be positive.");
			}

			m_depth = depth;

			m_unitNodeCount = unitNodeCount;
			m_rectTree = treeArea;

			m_treeOffsetX = -m_rectTree.x;
			m_treeOffsetY = -m_rectTree.y;
		}

		/// <summary>Returns the leaf node index containing <paramref name="point"/>.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="point"/> is outside the tree bounds.</exception>
		public long FindNodeIndex(Vector2 point)
		{
			if(!TryFindNodeIndex(point,out var index))
			{
				throw new ArgumentOutOfRangeException(nameof(point),point,"Point is outside the tree bounds.");
			}

			return index;
		}

		/// <summary>Attempts to resolve the leaf node index for <paramref name="point"/>.</summary>
		public bool TryFindNodeIndex(Vector2 point,out long index)
		{
			var treePoint = Convert2TreeCoordinate(point);

			if(!_ContainsTreePoint(treePoint))
			{
				index = 0;

				return false;
			}

			if(m_depth == 0)
			{
				index = c_rootLeafIndex;

				return true;
			}

			index = _FindNodeIndexRecursive(1,0,treePoint,m_rectTree);

			return true;
		}

		/// <summary>Returns the world-space rectangle of the leaf cell containing <paramref name="point"/>.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="point"/> is outside the tree bounds.</exception>
		public Rect FindNodeRect(Vector2 point)
		{
			var treePoint = Convert2TreeCoordinate(point);

			if(!_ContainsTreePoint(treePoint))
			{
				throw new ArgumentOutOfRangeException(nameof(point),point,"Point is outside the tree bounds.");
			}

			if(m_depth == 0)
			{
				return _ConvertRect2WorldCoordinate(m_rectTree);
			}

			var rect = _FindNodeRectRecursive(1,treePoint,m_rectTree);

			return _ConvertRect2WorldCoordinate(rect);
		}

		/// <summary>
		/// Reconstructs a cell rectangle from a path encoded as a string of 1-based child digits.
		/// </summary>
		/// <param name="nodeID">Per-level child selector; each character must be an ASCII digit from <c>1</c> to <c>unitNodeCount²</c>.</param>
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
				var digit = nodeID[i];

				if(digit < '1' || digit > '9')
				{
					throw new ArgumentException($"Invalid node ID character '{digit}' at position {i}. Use ASCII digits 1-{maxIndex}.",nameof(nodeID));
				}

				var index = digit-'0';

				if(index > maxIndex)
				{
					throw new ArgumentException($"Invalid node ID character '{digit}' at position {i}.",nameof(nodeID));
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

			return _ConvertRect2WorldCoordinate(rect);
		}

		/// <summary>Returns the width and height of a single leaf cell.</summary>
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

			throw new InvalidOperationException($"Point {point} is inside the tree bounds but does not resolve to a leaf cell.");
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

			throw new InvalidOperationException($"Point {point} is inside the tree bounds but does not resolve to a leaf cell.");
		}

		private void _FindNodesIndexInBoundRecursive(int depth,long parentIndex,Rect srcRect,Rect dstRect,List<long> resultList)
		{
			if(depth > m_depth)
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

		/// <summary>Returns world-space rectangles of all leaf cells overlapping <paramref name="rect"/>.</summary>
		public List<Rect> FindNodeRectInBound(Rect rect)
		{
			var treeRect = ConvertRect2TreeCoordinate(rect);
			var rectList = new List<Rect>();

			if(m_depth == 0)
			{
				if(m_rectTree.Overlaps(treeRect))
				{
					rectList.Add(_ConvertRect2WorldCoordinate(m_rectTree));
				}

				return rectList;
			}

			_FindNodesRectRecursive(1,0,m_rectTree,treeRect,rectList);

			return rectList;
		}

		private void _FindNodesRectRecursive(int depth,long parentIndex,Rect srcRect,Rect dstRect,List<Rect> resultList)
		{
			if(depth > m_depth)
			{
				resultList.Add(_ConvertRect2WorldCoordinate(srcRect));

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

		/// <summary>
		/// Moves a value from its old cell to the cell containing <paramref name="point"/> when the index changes.
		/// </summary>
		/// <returns>The new node index (unchanged when the point still maps to <paramref name="oldIndex"/>).</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="point"/> is outside the tree bounds.</exception>
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

		/// <summary>Stores <paramref name="value"/> in the leaf cell containing <paramref name="point"/>.</summary>
		/// <returns>The assigned node index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="point"/> is outside the tree bounds.</exception>
		public long AddValue(TValue value,Vector2 point)
		{
			lock(m_syncRoot)
			{
				var index = FindNodeIndex(point);

				_AddValueUnsafe(index,value);

				return index;
			}
		}

		/// <summary>Attempts to store <paramref name="value"/> when <paramref name="point"/> is inside the tree bounds.</summary>
		public bool TryAddValue(TValue value,Vector2 point,out long index)
		{
			lock(m_syncRoot)
			{
				if(!TryFindNodeIndex(point,out index))
				{
					return false;
				}

				_AddValueUnsafe(index,value);

				return true;
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

		/// <summary>Removes the first equal occurrence of <paramref name="value"/> from the cell at <paramref name="point"/>.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="point"/> is outside the tree bounds.</exception>
		public void RemoveValue(TValue value,Vector2 point)
		{
			lock(m_syncRoot)
			{
				var index = FindNodeIndex(point);

				_RemoveValueUnsafe(index,value);
			}
		}

		/// <summary>Attempts to remove <paramref name="value"/> when <paramref name="point"/> is inside the tree bounds.</summary>
		public bool TryRemoveValue(TValue value,Vector2 point)
		{
			lock(m_syncRoot)
			{
				if(!TryFindNodeIndex(point,out var index))
				{
					return false;
				}

				return _RemoveValueUnsafe(index,value);
			}
		}

		private bool _RemoveValueUnsafe(long key,TValue value)
		{
			if(!m_valueListDict.TryGetValue(key,out List<TValue> valueList))
			{
				return false;
			}

			if(!valueList.Remove(value))
			{
				return false;
			}

			if(valueList.Count == 0)
			{
				m_valueListDict.Remove(key);
			}

			return true;
		}

		/// <summary>Returns node indices of leaf cells overlapping <paramref name="rect"/>.</summary>
		public List<long> FindNodeInBound(Rect rect)
		{
			var indexList = new List<long>();
			var treeRect = ConvertRect2TreeCoordinate(rect);

			if(m_depth == 0)
			{
				if(m_rectTree.Overlaps(treeRect))
				{
					indexList.Add(c_rootLeafIndex);
				}

				return indexList;
			}

			_FindNodesIndexInBoundRecursive(1,0,m_rectTree,treeRect,indexList);

			return indexList;
		}

		/// <summary>Gathers all stored values whose cells overlap <paramref name="rect"/>.</summary>
		public List<TValue> GetValuesInBound(Rect rect)
		{
			lock(m_syncRoot)
			{
				var resultList = new List<TValue>();
				var indexList = new List<long>();
				var treeRect = ConvertRect2TreeCoordinate(rect);

				if(m_depth == 0)
				{
					if(m_rectTree.Overlaps(treeRect) && m_valueListDict.TryGetValue(c_rootLeafIndex,out var rootValues))
					{
						resultList.AddRange(rootValues);
					}

					return resultList;
				}

				_FindNodesIndexInBoundRecursive(1,0,m_rectTree,treeRect,indexList);

				for(var i=0;i<indexList.Count;i++)
				{
					var index = indexList[i];

					if(m_valueListDict.TryGetValue(index,out var valueList))
					{
						resultList.AddRange(valueList);
					}
				}

				return resultList;
			}
		}

		/// <summary>Maps a world-space point into the tree's internal coordinate system.</summary>
		public Vector2 Convert2TreeCoordinate(Vector2 point)
		{
			point.x += m_treeOffsetX;
			point.y += m_treeOffsetY;

			return point;
		}

		/// <summary>Maps an internal tree point back to world space.</summary>
		public Vector2 Convert2GeoCoordinate(Vector2 point)
		{
			point.x -= m_treeOffsetX;
			point.y -= m_treeOffsetY;

			return point;
		}

		/// <summary>Maps a world-space rectangle into tree-local coordinates.</summary>
		public Rect ConvertRect2TreeCoordinate(Rect rect)
		{
			rect.x += m_treeOffsetX;
			rect.y += m_treeOffsetY;

			return rect;
		}

		/// <summary>Flattens every stored value across all occupied cells into a new array.</summary>
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

		private bool _ContainsTreePoint(Vector2 treePoint)
		{
			return m_rectTree.Contains(treePoint);
		}

		private Rect _ConvertRect2WorldCoordinate(Rect rect)
		{
			rect.x -= m_treeOffsetX;
			rect.y -= m_treeOffsetY;

			return rect;
		}
	}
}