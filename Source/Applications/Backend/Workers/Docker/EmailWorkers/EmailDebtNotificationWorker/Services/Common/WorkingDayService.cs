using System;

namespace EmailDebtNotificationWorker.Services.Common
{
	/// <summary>
	/// Сервис для работы с рабочими днями и временем
	/// </summary>
	public class WorkingDayService : IWorkingDayService
	{
		private static readonly TimeSpan _workDayStart = TimeSpan.FromHours(9);
		private static readonly TimeSpan _workDayEnd = TimeSpan.FromHours(18);

		public bool IsOddWeekday(DateTime dateTime)
		{
			var moscowDateTime = GetMoscowDateTime(dateTime);

			return moscowDateTime.DayOfWeek
				is DayOfWeek.Monday
				or DayOfWeek.Wednesday
				or DayOfWeek.Friday;
		}

		public bool IsWithinWorkingHours(DateTime dateTime)
		{
			var moscowDateTime = GetMoscowDateTime(dateTime);
			TimeSpan timeOfDay = moscowDateTime.TimeOfDay;

			return timeOfDay >= _workDayStart
				&& timeOfDay < _workDayEnd;
		}

		public bool IsWorkingDay(DateTime dateTime)
		{
			var moscowDateTime = GetMoscowDateTime(dateTime);
			return moscowDateTime.DayOfWeek != DayOfWeek.Saturday
				&& moscowDateTime.DayOfWeek != DayOfWeek.Sunday;
		}

		public DateTime GetOptimalSendingTime(DateTime dateTime)
		{
			var moscowDateTime = GetMoscowDateTime(dateTime);

			if(IsOddWeekday(moscowDateTime) && IsWithinWorkingHours(moscowDateTime))
			{
				return moscowDateTime;
			}

			return GetNextWorkingTime(moscowDateTime);
		}

		public DateTime GetNextWorkingDay(DateTime dateTime)
		{
			var moscowDateTime = GetMoscowDateTime(dateTime);
			DateTime candidate = moscowDateTime.Date.AddDays(1);

			while(candidate.DayOfWeek 
				is DayOfWeek.Saturday 
				or DayOfWeek.Sunday)
			{
				candidate = candidate.AddDays(1);
			}

			return candidate;
		}

		public DateTime GetNextWorkingTime(DateTime dateTime)
		{
			var moscowDateTime = GetMoscowDateTime(dateTime);
			TimeSpan timeOfDay = moscowDateTime.TimeOfDay;

			if(!IsOddWeekday(moscowDateTime))
			{
				DateTime nextWorkingDay = GetNextWorkingDay(moscowDateTime);
				return nextWorkingDay.Date.Add(_workDayStart);
			}

			if(timeOfDay < _workDayStart)
			{
				return moscowDateTime.Date.Add(_workDayStart);
			}

			if(timeOfDay >= _workDayEnd)
			{
				DateTime nextDay = GetNextWorkingDay(moscowDateTime);
				return nextDay.Date.Add(_workDayStart);
			}

			return moscowDateTime;
		}

		private static DateTime GetMoscowDateTime(DateTime localDate)
		{
			var utcDateTime = localDate.ToUniversalTime();
			var moscowDateTime = utcDateTime.AddHours(3);

			return moscowDateTime;
		}
	}
}
