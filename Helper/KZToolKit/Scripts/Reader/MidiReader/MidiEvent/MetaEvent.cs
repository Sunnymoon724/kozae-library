using System;
using System.Collections.Generic;
using System.IO;

namespace KZLib.ToolKits
{
	public class MetaEvent : MidiEvent 
	{
		public byte MetaStatus => m_data1;

		public MetaEvent(byte status,byte data2,int delta) : base(0xFF,status,data2,delta,1) { }
		
		public override string ToString() 
		{
			return $"{AbsoluteTime} {MetaStatus}";
		}
	}

	public class MetaTextEvent : MetaEvent 
	{
		public string Text { get; }

		public MetaTextEvent(string text,byte status,int delta) : base(status,0x00,delta)
		{
			Text = text;
		}

		public MetaTextEvent(BinaryReader binaryReader,int length,int count,byte status,int delta) : base(status,0x00,delta)
		{
			if(length != count)
			{
				throw new ArgumentException($"value is not valid. {length} != {count}");
			}

			var byteList = new List<byte>();

			for(var i=0;i<count;i++)
			{
				byteList.Add(binaryReader.ReadByte());
			}

			Text = string.Join(":",byteList);
		}

		public override string ToString() 
		{
			return $"{base.ToString()} {Text}";
		}
	}

	public class TrackSequenceNumberEvent : MetaEvent
	{
		private readonly ushort m_sequenceNumber = 0;

		public TrackSequenceNumberEvent(BinaryReader binaryReader,int length,int delta) : base(0x00,0x00,delta)
		{
			if(length == 2)
			{
				m_sequenceNumber = (ushort)((binaryReader.ReadByte() << 8)+binaryReader.ReadByte());
			}
			else
			{
				binaryReader.ReadBytes(2);

				m_sequenceNumber = 0;
			}
		}
		
		public override string ToString()
		{
			return $"{base.ToString()} {m_sequenceNumber}";
		}
	}

	public class MetaDataEvent : MetaEvent
	{
		public byte[] DataArray { get; }

		public MetaDataEvent(byte status,byte[] dataArray,int delta) : base(status,0x00,delta)
		{
			DataArray = new byte[dataArray.Length];

			Array.Copy(dataArray,DataArray,DataArray.Length);
		}
	}

	public class TempoEvent : MetaEvent 
	{
		public int MicrosecondsPerQuarterNote { get; }		

		public TempoEvent(BinaryReader binaryReader,int length,int delta) : base(0x51,0x00,delta)
		{
			if(length != 3) 
			{
				throw new InvalidDataException("Tempo length is not 3");
			}

			MicrosecondsPerQuarterNote = (binaryReader.ReadByte() << 16) + (binaryReader.ReadByte() << 8) + binaryReader.ReadByte();
		}
		
		public override string ToString() 
		{
			return $"{base.ToString()} {60000000 / MicrosecondsPerQuarterNote}bpm ({MicrosecondsPerQuarterNote})";
		}
		
		// public double Tempo
		// {
		// 	get => (60000000.0/m_MicrosecondsPerQuarterNote);
		// 	set => m_MicrosecondsPerQuarterNote = (int) (60000000.0/value);
		// }
	}
}