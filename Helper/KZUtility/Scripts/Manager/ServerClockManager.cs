using System;
using KZLib.Utilities;

namespace KZLib
{
	/// <summary>
	/// Client clock aligned to server time. After <see cref="Sync"/>, <see cref="UtcNow"/> and <see cref="Now"/>
	/// extrapolate server time from local elapsed time instead of the device clock.
	/// </summary>
	public class ServerClockManager : Singleton<ServerClockManager>
	{
		/// <summary>
		/// Offset between local UTC and the last synced server time: UtcNow - serverTime.
		/// </summary>
		private TimeSpan m_timeDifference = TimeSpan.Zero;

		private ServerClockManager() { }

		/// <summary>
		/// Local UTC moment when the clock was last synced with the server.
		/// </summary>
		public DateTime LastSyncedAt { get; private set; }

		/// <summary>
		/// Current server time in UTC, adjusted by <see cref="m_timeDifference"/>.
		/// </summary>
		public DateTime UtcNow => DateTime.UtcNow-m_timeDifference;

		/// <summary>
		/// <see cref="UtcNow"/> converted to local time.
		/// </summary>
		public DateTime Now => UtcNow.ToLocalTime();

		protected override void _Initialize()
		{
			base._Initialize();

			LastSyncedAt = DateTime.UtcNow;
			m_timeDifference = TimeSpan.Zero;
		}

		protected override void _Release(bool disposing)
		{
			if(disposing)
			{
				LastSyncedAt = default;
				m_timeDifference = TimeSpan.Zero;
			}

			base._Release(disposing);
		}

		/// <summary>
		/// Returns <see cref="Now"/> when <paramref name="isLocal"/> is true; otherwise <see cref="UtcNow"/>.
		/// </summary>
		public DateTime GetNow(bool isLocal = true)
		{
			return isLocal ? Now : UtcNow;
		}

		/// <summary>
		/// Syncs from a Unix timestamp in milliseconds (UTC).
		/// </summary>
		public void Sync(long newServerTimestamp)
		{
			var dateTime = DateTime.UnixEpoch.AddMilliseconds(newServerTimestamp);

			Sync(dateTime);
		}

		/// <summary>
		/// Syncs from server time. <paramref name="newServerTime"/> must be UTC
		/// (same basis as <see cref="DateTime.UtcNow"/>).
		/// </summary>
		public void Sync(DateTime newServerTime)
		{
			m_timeDifference = DateTime.UtcNow-newServerTime;

			LastSyncedAt = DateTime.UtcNow;
		}
	}
}
