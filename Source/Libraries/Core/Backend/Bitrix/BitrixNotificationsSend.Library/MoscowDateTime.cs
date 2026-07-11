using System;

namespace BitrixNotificationsSend.Library
{
	/// <summary>
	/// Текущие дата и время по московскому времени
	/// </summary>
	public static class MoscowDateTime
	{
		private static readonly TimeZoneInfo _moscowTimeZone = GetMoscowTimeZone();

		private static TimeZoneInfo GetMoscowTimeZone()
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
			}
			catch(TimeZoneNotFoundException)
			{
				return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
			}
		}

		/// <summary>
		/// Текущие дата и время по Москве
		/// </summary>
		public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _moscowTimeZone);

		/// <summary>
		/// Текущая дата по Москве
		/// </summary>
		public static DateTime Today => Now.Date;
	}
}
