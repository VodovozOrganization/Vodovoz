namespace EmailDebtNotificationWorker.Services
{
	/// <summary>
	/// Сервис для работы с рабочими днями и временем
	/// </summary>
	public class WorkingDayService : IWorkingDayService
	{
		private static readonly TimeSpan _workDayStart = TimeSpan.FromHours(9);
		private static readonly TimeSpan _workDayEnd = TimeSpan.FromHours(18);

		public bool IsWorkingDay(DateTime date)
		{
			return date.DayOfWeek >= DayOfWeek.Monday &&
				   date.DayOfWeek <= DayOfWeek.Friday;
		}

		public bool IsWithinWorkingHours(DateTime dateTime)
		{
			TimeSpan timeOfDay = dateTime.TimeOfDay;
			return timeOfDay >= _workDayStart && timeOfDay < _workDayEnd;
		}

		public DateTime GetOptimalSendingTime(DateTime desiredTime)
		{
			if(IsWorkingDay(desiredTime) && IsWithinWorkingHours(desiredTime))
			{
				return desiredTime;
			}

			return GetNextWorkingTime(desiredTime);
		}

		public DateTime GetNextWorkingDay(DateTime fromDate)
		{
			DateTime candidate = fromDate.Date.AddDays(1);

			while(candidate.DayOfWeek == DayOfWeek.Saturday ||
				  candidate.DayOfWeek == DayOfWeek.Sunday)
			{
				candidate = candidate.AddDays(1);
			}

			return candidate;
		}

		public DateTime GetNextWorkingTime(DateTime fromDateTime)
		{
			if(!IsWorkingDay(fromDateTime))
			{
				DateTime nextWorkingDay = GetNextWorkingDay(fromDateTime);
				return nextWorkingDay.Date.Add(_workDayStart);
			}

			TimeSpan timeOfDay = fromDateTime.TimeOfDay;

			if(timeOfDay < _workDayStart)
			{
				return fromDateTime.Date.Add(_workDayStart);
			}

			if(timeOfDay >= _workDayEnd)
			{
				DateTime nextDay = GetNextWorkingDay(fromDateTime);
				return nextDay.Date.Add(_workDayStart);
			}

			return fromDateTime;
		}
	}
}
