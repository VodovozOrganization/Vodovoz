using System;
using System.Collections.Generic;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Расписание повторных попыток отправки в Mindbox из настройки Mindbox.RetryScheduleMinutes.
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

		public static TimeSpan[] ParseIntervals(string value)
		{
			var minutes = ParseMinutes(value);
			var intervals = new TimeSpan[minutes.Length];

			for(var i = 0; i < minutes.Length; i++)
			{
				intervals[i] = TimeSpan.FromMinutes(minutes[i]);
			}

			return intervals;
		}
	}
}
