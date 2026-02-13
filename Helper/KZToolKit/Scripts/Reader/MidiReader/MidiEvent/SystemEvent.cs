using System;

namespace KZLib.ToolKits
{
	public class SystemEvent : MidiEvent 
	{
		private readonly byte[] m_dataArray = Array.Empty<byte>();

		public SystemEvent(byte[] dataArray,int delta,int channel) : base(0xF0,0x00,0x00,delta,channel)
		{
			m_dataArray = new byte[dataArray.Length];

			Array.Copy(dataArray,m_dataArray,m_dataArray.Length);
		}
	}
}