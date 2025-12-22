using System;

namespace KZLib.KZTool
{
	public class MidiEvent
	{
		private int m_channel = 0;
		public byte CommandCode { get; protected set; }
		public int DeltaTime { get; }
		public long AbsoluteTime { get; set; }

		protected byte m_data1 = 0x00;
		protected byte m_data2 = 0x00;

		public MidiEvent(byte command,byte data1,byte data2,int delta,int channel)
		{
			CommandCode = command;
			Channel = channel;
			DeltaTime = delta;

			m_data1 = data1;
			m_data2 = data2;
		}

		public virtual int Channel
		{
			get => m_channel;
			set
			{
				if((value < 1) || (value > 16))
				{
					throw new ArgumentOutOfRangeException("value",value,$"Channel must be 1-16 (Got {value})");
				}

				m_channel = value;
			}
		}

		public static bool IsEndTrack(MidiEvent midiEvent)
		{
			if(midiEvent == null)
			{
				return false;
			}

			if(midiEvent is MetaEvent metaEvent)
			{
				return metaEvent.MetaStatus == 0x2F;
			}

			return false;
		}

		public override string ToString()
		{
			return (CommandCode >= 0xF0) ? $"{AbsoluteTime} {CommandCode}" : $"{AbsoluteTime} {CommandCode} Ch: {m_channel}";
		}
	}
}