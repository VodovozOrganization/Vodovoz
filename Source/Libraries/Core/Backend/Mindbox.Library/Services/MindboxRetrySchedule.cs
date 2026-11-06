using System;
using System.Collections.Generic;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Расписание повторных попыток отправки в Mindbox из настроек:
	/// базовые интервалы + повтор раз в N минут в течение заданного времени.
	/// </summary>
	public static class MindboxRetrySchedule
	{
		private static readonly int[] _defaultSchedule = { 1, 5, 15, 60 };

		public static int[] ParseMinutes(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
			{
				return _defaultSchedule;
			}

			var minutes = new List<int>();

			foreach(var part in value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if(int.TryParse(part.Trim(), out var minute) && minute > 0)
				{
					minutes.Add(minute);
				}
			}

			return minutes.Count > 0
				? minutes.ToArray()
				: _defaultSchedule;
		}

		/// <summary>
		/// Базовые интервалы + интервалы "раз в N минут" на заданное число часов.
		/// </summary>
		public static TimeSpan[] BuildIntervals(
			string scheduleMinutes,
			int retryIntervalMinutes,
			int retryMaxHours)
		{
			var minutes = new List<int>(ParseMinutes(scheduleMinutes));

			var intervalMinutes = retryIntervalMinutes > 0 ? retryIntervalMinutes : 60;
			var maxHours = retryMaxHours > 0 ? retryMaxHours : 24;

			for(var hour = 0; hour < maxHours; hour++)
			{
				minutes.Add(intervalMinutes);
			}

			var intervals = new TimeSpan[minutes.Count];

			for(var i = 0; i < minutes.Count; i++)
			{
				intervals[i] = TimeSpan.FromMinutes(minutes[i]);
			}

			return intervals;
		}

		public static int GetMaxAttempts(
			string scheduleMinutes,
			int retryIntervalMinutes,
			int retryMaxHours)
		{
			return BuildIntervals(scheduleMinutes, retryIntervalMinutes, retryMaxHours).Length;
		}
	}
}
