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
			var moscowTime = ConvertToMoscowTime(date);

			return moscowTime.DayOfWeek >= DayOfWeek.Monday &&
				   moscowTime.DayOfWeek <= DayOfWeek.Friday;
		}

		public bool IsWithinWorkingHours(DateTime dateTime)
		{
			var moscowTime = ConvertToMoscowTime(dateTime);
			var timeOfDay = moscowTime.TimeOfDay;

			return timeOfDay >= _workDayStart && timeOfDay < _workDayEnd;
		}

		public DateTime GetOptimalSendingTime(DateTime desiredTime)
		{
			var moscowDesiredTime = ConvertToMoscowTime(desiredTime);

			if(IsWorkingDay(moscowDesiredTime) && IsWithinWorkingHours(moscowDesiredTime))
			{
				return desiredTime;
			}

			return GetNextWorkingTime(desiredTime);
		}

		public DateTime GetNextWorkingDay(DateTime fromDate)
		{
			var moscowDate = ConvertToMoscowTime(fromDate).Date;
			var candidate = moscowDate.AddDays(1);

			while(candidate.DayOfWeek == DayOfWeek.Saturday ||
				   candidate.DayOfWeek == DayOfWeek.Sunday)
			{
				candidate = candidate.AddDays(1);
			}

			return ConvertFromMoscowTime(candidate);
		}

		public DateTime GetNextWorkingTime(DateTime fromDateTime)
		{
			var moscowTime = ConvertToMoscowTime(fromDateTime);

			if(!IsWorkingDay(moscowTime))
			{
				var nextWorkingDay = GetNextWorkingDay(fromDateTime);

				return ConvertToMoscowTime(nextWorkingDay).Date.Add(_workDayStart);
			}

			var timeOfDay = moscowTime.TimeOfDay;

			if(timeOfDay < _workDayStart)
			{
				return moscowTime.Date.Add(_workDayStart);
			}

			if(timeOfDay >= _workDayEnd)
			{
				var nextDay = GetNextWorkingDay(ConvertFromMoscowTime(moscowTime));
				return ConvertToMoscowTime(nextDay).Date.Add(_workDayStart);
			}

			return moscowTime;
		}

		private static DateTime ConvertToMoscowTime(DateTime utcTime) => utcTime.AddHours(3);

		private static DateTime ConvertFromMoscowTime(DateTime moscowTime) => moscowTime.AddHours(-3);
	}
}
