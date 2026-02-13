using System;

namespace KZLib.Data
{
	public struct CircularIndex
	{
		private readonly int m_entityCount;
		private readonly bool m_isClockWise;
		private int m_currentIndex;

		public int CurrentIndex => m_currentIndex;
		public int EntityCount => m_entityCount;
		public bool IsClockWise => m_isClockWise;

		public CircularIndex(int entityCount,int currentIndex,bool isClockWise)
		{
			m_isClockWise = isClockWise;

			if(entityCount <= 0)
			{
				throw new ArgumentOutOfRangeException("entityCount must be greater than zero.");
			}

			m_entityCount = entityCount;

			_SetCurrentIndex(currentIndex);
		}

		public void SetCurrentIndex(int index)
		{
			_SetCurrentIndex(index);
		}

		private void _SetCurrentIndex(int index)
		{
			if(index < 0 || index >= m_entityCount)
			{
				throw new ArgumentOutOfRangeException($"{index} is out of range of {m_entityCount} [0 <= {index} < {m_entityCount}]");
			}

			m_currentIndex = index;
		}

		public int MoveLeft()
		{
			m_currentIndex = MoveLeft(m_currentIndex);

			return m_currentIndex;
		}

		public int MoveLeft(int index)
		{
			return m_isClockWise ? _GetCurrentLeftIndex(index) : _GetCurrentRightIndex(index);
		}

		public int MoveRight()
		{
			m_currentIndex = MoveRight(m_currentIndex);

			return m_currentIndex;
		}

		public int MoveRight(int index)
		{
			return m_isClockWise ? _GetCurrentRightIndex(index) : _GetCurrentLeftIndex(index);
		}

		private int _GetCurrentLeftIndex(int current)
		{
			return _LoopClamp(current-1);
		}

		private int _GetCurrentRightIndex(int current)
		{
			return _LoopClamp(current+1);
		}

		private int _LoopClamp(int index)
		{
			return (index%m_entityCount+m_entityCount)%m_entityCount;
		}
	}
}