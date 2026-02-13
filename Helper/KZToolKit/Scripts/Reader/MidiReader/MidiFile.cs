using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using KZHelper.ToolKits;

namespace KZLib.ToolKits
{
	public class MidiFile
	{
		private const string c_fileHeaderChunk = "MThd";
		private const string c_fileTrackChunk = "MTrk";		

		private List<MidiTrack> m_midiTrackList = new();

		public IEnumerable<MidiTrack> MidiTrackGroup => m_midiTrackList;

		public int DeltaTicksPerQuarterNote => m_deltaTicksPerQuarterNote;

		private short m_deltaTicksPerQuarterNote = 0;

		public MidiFile(string fullPath)
		{
			using var reader = new BinaryReader(File.OpenRead(fullPath));

			_LoadMidiFile(reader);
		}

		private void _LoadMidiFile(BinaryReader binaryReader)
		{
			var chunkHeader = Encoding.UTF8.GetString(binaryReader.ReadBytes(4));

			if(chunkHeader != c_fileHeaderChunk)
			{
				throw new NullReferenceException("HeaderChunk is not found.");
			}

			var chunkSize = CommonUtility.ReadInt32(binaryReader);

			if(chunkSize != 6)
			{
				throw new ArgumentException("HeaderChunk is not 6 bytes.");
			}

			int fileFormat = CommonUtility.ReadInt16(binaryReader); // 0 = single track, 1 = multi track synchronous, 2 = multi track asynchronous
			int trackCount = CommonUtility.ReadInt16(binaryReader);

			m_deltaTicksPerQuarterNote = CommonUtility.ReadInt16(binaryReader);
			m_midiTrackList = new List<MidiTrack>(trackCount);

			for(var i=0;i<trackCount;i++)
			{
				m_midiTrackList.Add(ReadTrack(binaryReader));
			}
		}

		private MidiTrack ReadTrack(BinaryReader binaryReader)
		{
			var eventList = new List<MidiEvent>();
			var totalTime = 0L;

			var chunkHeader = Encoding.UTF8.GetString(binaryReader.ReadBytes(4));

			if(chunkHeader != c_fileTrackChunk)
			{
				throw new NullReferenceException("TrackChunk is not found.");
			}

			var chunkSize = CommonUtility.ReadInt32(binaryReader);
			var startPosition = binaryReader.BaseStream.Position;
			var endPosition = startPosition+chunkSize;

			MidiEvent nowEvent = null!;

			while(binaryReader.BaseStream.Position < endPosition)
			{
				nowEvent = _ReadNextEvent(binaryReader,nowEvent);
				totalTime += nowEvent.DeltaTime;
				
				nowEvent.AbsoluteTime = totalTime;

				eventList.Add(nowEvent);
			}

			if(binaryReader.BaseStream.Position != endPosition)
			{
				throw new ArgumentException("TrackChunk length is not correct.");
			}

			return new MidiTrack(eventList,totalTime);
		}

		private MidiEvent _ReadNextEvent(BinaryReader binaryReader,MidiEvent previous)
		{
			var deltaTime = _ReadVariableLength(binaryReader);
			var channel = 1;
			var status = binaryReader.ReadByte();

			byte code;

			if((status & 0x80) == 0)
			{
				code = previous.CommandCode;
				channel = previous.Channel;
				binaryReader.BaseStream.Position--;
			}
			else
			{
				if((status & 0xF0) == 0xF0)
				{
					code = status;
				}
				else
				{
					code = (byte)(status & 0xF0);
					channel = (status & 0x0F)+1;
				}
			}

			switch(code)
			{
				case 0x90: //NoteOn
					return new NoteEvent(code,binaryReader.ReadByte(),binaryReader.ReadByte(),deltaTime,channel);
				case 0x80: //NoteOff
				case 0xA0: //AfterTouch
				case 0xB0: //ControlChange
				case 0xE0: //PitchWheel
					return new MidiEvent(code,binaryReader.ReadByte(),binaryReader.ReadByte(),deltaTime,channel);
				case 0xC0: //ProgramChange
				case 0xD0: //ChannelPressure
					return new MidiEvent(code,binaryReader.ReadByte(),0x00,deltaTime,channel);
				case 0xF8: //TimingClock
				case 0xFA: //StartSequence
				case 0xFB: //ContinueSequence
				case 0xFC: //StopSequence
					return new MidiEvent(code,0x00,0x00,deltaTime,channel);
				case 0xF0: //StartOfSystemExclusiveMessage
					return _ReadSystemEvent(binaryReader,deltaTime,channel);
				case 0xFF: //MetaEvent
					return _ReadMetaEvent(binaryReader,deltaTime);
				default:
					throw new NotSupportedException($"Not supported event [{code:X2}]");
			}
		}

		private MidiEvent _ReadSystemEvent(BinaryReader binaryReader,int deltaTime,int channel)
		{
			var dataList = new List<byte>();
			var status = binaryReader.ReadByte();

			while(status != 0xF7)
			{
				dataList.Add(status);
				status = binaryReader.ReadByte();
			}

			return new SystemEvent(dataList.ToArray(),deltaTime,channel);
		}

		private MidiEvent _ReadMetaEvent(BinaryReader binaryReader,int deltaTime)
		{
			var status = binaryReader.ReadByte();
			int length = _ReadVariableLength(binaryReader);

			switch(status) 
			{
				case 0x00: //TrackSequenceNumber
					return new TrackSequenceNumberEvent(binaryReader,length,deltaTime);
				case 0x01: //TextEvent
				case 0x02: //Copyright
				case 0x03: //SequenceTrackName
				case 0x04: //TrackInstrumentName
				case 0x05: //Lyric
				case 0x06: //Marker
				case 0x07: //CuePoint
				case 0x08: //ProgramName
				case 0x09: //DeviceName
					return new MetaTextEvent(Encoding.UTF8.GetString(binaryReader.ReadBytes(length),0,length),status,deltaTime);
				case 0x2F: //EndTrack
					if(length != 0)
					{
						// TBN Change do nothing with this information but no exception
						binaryReader.ReadBytes(length);
					}
					return new MetaEvent(status,0x00,deltaTime);
				case 0x51: //SetTempo
					return new TempoEvent(binaryReader,length,deltaTime);
				case 0x58: //TimeSignature
					return new MetaTextEvent(binaryReader,length,4,status,deltaTime);
				case 0x59: //KeySignature
					return new MetaTextEvent(binaryReader,length,2,status,deltaTime);
				case 0x7F: //SequencerSpecific
					return new MetaDataEvent(status,binaryReader.ReadBytes(length),deltaTime);
				case 0x54: //SmpteOffset
					return new MetaTextEvent(binaryReader,length,5,status,deltaTime);
				default:
				
					var data = binaryReader.ReadBytes(length);

					if(data.Length != length)
					{
						throw new Exception("메타 이벤트의 데이터를 완전히 읽지 못했습니다.");
					}

					return new MetaDataEvent(status,data,deltaTime);
			}
		}

		private int _ReadVariableLength(BinaryReader binaryReader)
		{
			var value = 0;
			int next;

			do
			{
				next = binaryReader.ReadByte();
				value <<= 7;
				value |= next & 0x7F;

			}while((next & 0x80) == 0x80);

			return value;
		}
	}
}
