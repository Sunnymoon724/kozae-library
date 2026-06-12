using System;
using KZLib;

/// <summary>
/// Time helpers built on <see cref="ServerClockManager"/> and pure <see cref="DateTime"/> utilities.
/// </summary>
public static class KZTimeKit
{
	/// <summary>
	/// Returns whether <paramref name="now"/> is at or past <paramref name="target"/>.
	/// Both values must use the same basis (local or UTC).
	/// </summary>
	public static bool IsPassed(DateTime now,DateTime target)
	{
		return now >= target;
	}

	/// <summary>
	/// Remaining time until <paramref name="endTime"/>.
	/// Returns a negative span when the end time is already in the past.
	/// </summary>
	public static TimeSpan GetRemainUntil(DateTime endTime,bool isLocal)
	{
		var currentTime = ServerClockManager.In.GetNow(isLocal);
		var normalizedEndTime = Normalize(endTime,isLocal);

		return normalizedEndTime-currentTime;
	}

	/// <summary>
	/// Remaining time until a Unix timestamp in milliseconds (UTC epoch).
	/// </summary>
	public static TimeSpan GetRemainUntil(long timeStamp,bool isLocal)
	{
		var dateTime = DateTime.UnixEpoch.AddMilliseconds(timeStamp);

		return GetRemainUntil(isLocal ? dateTime.ToLocalTime() : dateTime,isLocal);
	}

	/// <summary>
	/// Remaining time until <paramref name="endTime"/>.
	/// When <paramref name="endTime"/> is null, uses <see cref="GetTomorrowMidnight"/>.
	/// Returns a negative span when the end time is already in the past.
	/// </summary>
	public static TimeSpan GetRemainUntil(bool isLocal,DateTime? endTime = null)
	{
		return GetRemainUntil(endTime ?? GetTomorrowMidnight(isLocal),isLocal);
	}

	/// <summary>
	/// Clamps a remain span to zero when the target time has passed.
	/// </summary>
	public static TimeSpan ClampRemainTime(TimeSpan remainTime)
	{
		return remainTime > TimeSpan.Zero ? remainTime : TimeSpan.Zero;
	}

	/// <summary>
	/// Returns whether the synced clock is within [<paramref name="startTime"/>, <paramref name="endTime"/>] (inclusive).
	/// </summary>
	public static bool IsWithinPeriod(DateTime startTime,DateTime endTime,bool isLocal = true)
	{
		var currentTime = ServerClockManager.In.GetNow(isLocal);
		var normalizedStartTime = Normalize(startTime,isLocal);
		var normalizedEndTime = Normalize(endTime,isLocal);

		return currentTime >= normalizedStartTime && currentTime <= normalizedEndTime;
	}

	/// <summary>
	/// Midnight at the start of the day after the synced clock's current calendar date.
	/// </summary>
	public static DateTime GetTomorrowMidnight(bool isLocal)
	{
		var currentTime = ServerClockManager.In.GetNow(isLocal);

		return currentTime.Date.AddDays(1);
	}

	/// <summary>
	/// Midnight on a matching weekday. When <paramref name="skipTodayIfMatch"/> is true and today
	/// already matches <paramref name="weekday"/>, returns the same weekday seven days later.
	/// </summary>
	public static DateTime GetWeekdayMidnight(DayOfWeek weekday,bool isLocal = true,bool skipTodayIfMatch = true)
	{
		var currentTime = ServerClockManager.In.GetNow(isLocal);
		var todayMidnight = currentTime.Date;

		var todayDayOfWeek = (int)todayMidnight.DayOfWeek;
		var targetDayOfWeek = (int)weekday;

		var daysUntilTargetDay = (targetDayOfWeek-todayDayOfWeek+7)%7;

		if(skipTodayIfMatch && daysUntilTargetDay == 0)
		{
			daysUntilTargetDay = 7;
		}

		return todayMidnight.AddDays(daysUntilTargetDay);
	}

	/// <summary>
	/// Returns whether <paramref name="dateTime"/>.Hour falls in a half-open hour range.
	/// When <paramref name="startHour"/> &lt;= <paramref name="endHour"/>, the range is
	/// [<paramref name="startHour"/>, <paramref name="endHour"/>).
	/// Otherwise the range wraps midnight, e.g. 22-6 covers 22:00-05:59.
	/// </summary>
	public static bool IsTimeInRange(DateTime dateTime,int startHour,int endHour)
	{
		var hour = dateTime.Hour;

		return (startHour <= endHour) ? (hour >= startHour && hour < endHour) : (hour >= startHour || hour < endHour);
	}

	private static DateTime Normalize(DateTime dateTime,bool isLocal)
	{
        return dateTime.Kind switch
        {
            DateTimeKind.Local => isLocal ? dateTime : dateTime.ToUniversalTime(),
            DateTimeKind.Utc => isLocal ? dateTime.ToLocalTime() : dateTime,
            _ => isLocal ? DateTime.SpecifyKind(dateTime, DateTimeKind.Local) : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        };
    }
}