namespace EmailDebtNotificationWorker.Services
{
	public interface IWorkingDayService
	{
		/// <summary>
		/// Рабочий ли день (Пн-Пт)
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		bool IsWorkingDay(DateTime date);

		/// <summary>
		/// Рабочее ли время (9:00-18:00)
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		bool IsWithinWorkingHours(DateTime dateTime);

		/// <summary>
		/// Получить оптимальное время отправки
		/// </summary>
		/// <param name="currentTime"></param>
		/// <returns></returns>
		DateTime GetOptimalSendingTime(DateTime currentTime);

		/// <summary>
		/// Получить следующий рабочий день (Пн-Пт)
		/// </summary>
		/// <param name="fromDate"></param>
		/// <returns></returns>
		DateTime GetNextWorkingDay(DateTime fromDate);

		/// <summary>
		/// Получить следующее рабочее время (следующий Пн-Пт 9:00-18:00)
		/// </summary>
		/// <param name="fromDateTime"></param>
		/// <returns></returns>
		DateTime GetNextWorkingTime(DateTime fromDateTime);
	}
}
