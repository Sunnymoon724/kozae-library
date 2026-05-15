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
		private const int c_sequenceNumberLength = 2;
		private const int c_byte1Shift = 8;

		private readonly ushort m_sequenceNumber = 0;

		public TrackSequenceNumberEvent(BinaryReader binaryReader,int length,int delta) : base(0x00,0x00,delta)
		{
			if(length == c_sequenceNumberLength)
			{
				m_sequenceNumber = (ushort)((binaryReader.ReadByte() << c_byte1Shift)+binaryReader.ReadByte());
			}
			else
			{
				binaryReader.ReadBytes(c_sequenceNumberLength);

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
		private const int c_tempoEventLength = 3;
		private const int c_byte2Shift = 16;
		private const int c_byte1Shift = 8;
		private const int c_microsecondsPerMinute = 60000000;

		public int MicrosecondsPerQuarterNote { get; }		

		public TempoEvent(BinaryReader binaryReader,int length,int delta) : base(0x51,0x00,delta)
		{
			if(length != c_tempoEventLength) 
			{
				throw new InvalidDataException("Tempo length is not 3");
			}

			MicrosecondsPerQuarterNote = (binaryReader.ReadByte() << c_byte2Shift) + (binaryReader.ReadByte() << c_byte1Shift) + binaryReader.ReadByte();
		}
		
		public override string ToString() 
		{
			return $"{base.ToString()} {c_microsecondsPerMinute / MicrosecondsPerQuarterNote}bpm ({MicrosecondsPerQuarterNote})";
		}
		
		// public double Tempo
		// {
		// 	get => (60000000.0/m_MicrosecondsPerQuarterNote);
		// 	set => m_MicrosecondsPerQuarterNote = (int) (60000000.0/value);
		// }
	}
}