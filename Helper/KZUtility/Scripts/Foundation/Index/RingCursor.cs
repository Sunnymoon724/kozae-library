using System;

namespace KZLib.Utilities
{
	/// <summary>
	/// Mutable cursor that moves left or right on a fixed-size ring of slot indices in <c>[0, <see cref="SlotCount"/>)</c>.
	/// </summary>
	/// <remarks>
	/// Pair with a parallel value array keyed by <see cref="CurrentIndex"/>.
	/// <see cref="Clockwise"/> maps visual left/right to index direction.
	/// </remarks>
	public struct RingCursor
	{
		private readonly int m_slotCount;
		private readonly bool m_clockwise;
		private int m_currentIndex;

		public int CurrentIndex => m_currentIndex;
		public int SlotCount => m_slotCount;
		public bool Clockwise => m_clockwise;

		public RingCursor(int slotCount,int currentIndex,bool clockwise)
		{
			m_clockwise = clockwise;

			if(slotCount <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(slotCount),"slotCount must be greater than zero.");
			}

			m_slotCount = slotCount;

			_SetCurrentIndex(currentIndex);
		}

		public void SetCurrentIndex(int index)
		{
			_SetCurrentIndex(index);
		}

		private void _SetCurrentIndex(int index)
		{
			if(index < 0 || index >= m_slotCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index),$"{index} is out of range of {m_slotCount} [0 <= {index} < {m_slotCount}]");
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
			return m_clockwise ? _GetLeftIndex(index) : _GetRightIndex(index);
		}

		public int MoveRight()
		{
			m_currentIndex = MoveRight(m_currentIndex);

			return m_currentIndex;
		}

		public int MoveRight(int index)
		{
			return m_clockwise ? _GetRightIndex(index) : _GetLeftIndex(index);
		}

		private int _GetLeftIndex(int current)
		{
			return _LoopClamp(current-1);
		}

		private int _GetRightIndex(int current)
		{
			return _LoopClamp(current+1);
		}

		private int _LoopClamp(int index)
		{
			return (index%m_slotCount+m_slotCount)%m_slotCount;
		}
	}
}
