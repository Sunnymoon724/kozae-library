using System.Collections.Generic;

namespace KZLib.KZTool
{
	public record MidiTrack
	{
		public long AbsoluteTime { get; }
		private readonly List<MidiEvent> m_eventList = new();

		public IEnumerable<MidiEvent> EventGroup => m_eventList;
		
		public MidiTrack(List<MidiEvent> eventList,long time)
		{
			AbsoluteTime = time;

			m_eventList.AddRange(eventList);
		}

		public int EventCount => m_eventList.Count;
	}
}