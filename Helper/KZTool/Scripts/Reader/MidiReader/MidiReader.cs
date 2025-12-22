using System.Collections.Generic;
using System.Linq;

namespace KZLib.KZTool
{
	public record MidiEventInfo
	{
		public int IndexTrack { get; }
		public float RealTime { get; }
		public MidiEvent MidiEvent { get; }
		
		public MidiEventInfo(int indexTrack,float realTime,MidiEvent midiEvent)
		{
			IndexTrack = indexTrack;
			RealTime = realTime;
			MidiEvent = midiEvent;
		}
	}

	public class MidiReader
	{
		private record TempoInfo
		{
			public int Track { get; }
			public long FromTick { get; }
			public double Cumulative { get; }
			public double Ratio { get; }
			public int MicrosecondsPerQuarterNote { get; }
			
			public TempoInfo(int track,long fromTick,double cumulative,double ratio,int microsecondsPerQuarterNote)
			{
				Track = track;
				FromTick = fromTick;
				Cumulative = cumulative;
				Ratio = ratio;
				MicrosecondsPerQuarterNote = microsecondsPerQuarterNote;
			}
		}

		private MidiFile m_midiFile = null!;

		private List<MidiEventInfo> m_eventInfoList = null!;

		public IEnumerable<MidiEventInfo> MidiEventGroup => m_eventInfoList;

		public bool LoadMidiFile(string fullPath)
		{
			m_midiFile = new MidiFile(fullPath);

			if(m_midiFile == null)
			{
				return false;
			}

			m_eventInfoList = _GetMidiEventList();

			return true;
		}

		private List<MidiEventInfo> _GetMidiEventList()
		{
			var tempoDataList = GetTempoDataList();
			var eventInfoList = new List<MidiEventInfo>();
			var tempo = 0;
			var index = 0;

			var trackIterator = m_midiFile.MidiTrackGroup.GetEnumerator();

			while(trackIterator.MoveNext())
			{
				if(trackIterator.Current is not MidiTrack midiTrack)
				{
					continue;
				}

				var iterator = midiTrack.EventGroup.GetEnumerator();

				tempo = 0;

				while(iterator.MoveNext())
				{
					var midiEvent = iterator.Current;

					while(tempo < tempoDataList.Count-1 && tempoDataList[tempo+1].FromTick < midiEvent.AbsoluteTime)
					{
						tempo++;
					}

					var newTime = tempoDataList[tempo].Cumulative+(midiEvent.AbsoluteTime-tempoDataList[tempo].FromTick)*tempoDataList[tempo].Ratio;

					if(midiEvent.CommandCode != 0x90)
					{
						continue;
					}

					eventInfoList.Add(new MidiEventInfo(index,(float)newTime,midiEvent));
                }

				index++;
			}

			return eventInfoList.Count == 0 ? eventInfoList : eventInfoList.OrderBy(x=>x.MidiEvent.AbsoluteTime).ToList();
		}

		private List<TempoInfo> GetTempoDataList()
		{
			var tempoInfoList = new List<TempoInfo>();
			var index = 0;
			var trackIterator = m_midiFile.MidiTrackGroup.GetEnumerator();

			while(trackIterator.MoveNext())
			{
				var track = trackIterator.Current;

				if(track == null)
				{
					continue;
				}

				var iterator = track.EventGroup.GetEnumerator();

				while(iterator.MoveNext())
				{
					if(iterator.Current is not TempoEvent tempoEvent || tempoInfoList.Count >= 1)
					{
						continue;
					}

					tempoInfoList.Add(new TempoInfo(index,tempoEvent.AbsoluteTime,tempoInfoList.Count > 0 ? tempoInfoList[^1].Cumulative+tempoEvent.DeltaTime*tempoInfoList[^1].Ratio : 0.0,tempoEvent.MicrosecondsPerQuarterNote/(double)m_midiFile.DeltaTicksPerQuarterNote/1000.0,tempoEvent.MicrosecondsPerQuarterNote));
				}

				index++;
			}

			if(tempoInfoList.Count == 0)
			{
				tempoInfoList.Add(new TempoInfo(0,0L,0.0,500.0/m_midiFile.DeltaTicksPerQuarterNote,500000));
			}
			else
			{
				if(tempoInfoList.Count > 1)
				{
					tempoInfoList = tempoInfoList.OrderBy(x=>x.FromTick).ToList();
				}
			}

			return tempoInfoList;
		}
	}
}