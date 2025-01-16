using System.Collections.Generic;
using System.Diagnostics;

namespace KZLib.KZUtility
{
	public class TimerMgr : Singleton<TimerMgr>
	{
		private readonly Dictionary<string,Stopwatch> m_timerDict = new Dictionary<string,Stopwatch>();

		private bool m_disposed = false;

		protected override void Release(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				m_timerDict.Clear();
			}

			m_disposed = true;

			base.Release(disposing);
		}

		public bool StartTimer(string key,bool isOverlap = true)
		{
			if(m_timerDict.ContainsKey(key) && !isOverlap)
			{
				return false;
			}

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			m_timerDict[key] = stopwatch;

			return true;
		}

		public bool GetTime(string key,out double seconds)
		{
			if(m_timerDict.TryGetValue(key,out var stopwatch))
			{
				seconds = stopwatch.Elapsed.TotalSeconds;

				return true;
			}
			else
			{
				seconds = 0.0d;

				return false;
			}
		}

		public bool StopTimer(string key)
		{
			if(m_timerDict.TryGetValue(key,out var stopwatch))
			{
				stopwatch.Stop();

				return true;
			}

			return false;
		}

		public bool IsTimerActive(string key)
		{
			return m_timerDict.ContainsKey(key);
		}
	}
}