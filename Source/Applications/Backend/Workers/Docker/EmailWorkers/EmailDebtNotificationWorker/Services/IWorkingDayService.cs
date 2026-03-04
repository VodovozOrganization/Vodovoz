using System;

namespace EmailDebtNotificationWorker.Services
{
	public interface IWorkingDayService
	{
		/// <summary>
		/// Проверяет, является ли дата рабочим днём (Пн-Пт) по московскому времени
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns>True если рабочий день (Пн-Пт) по московскому времени</returns>
		bool IsWorkingDay(DateTime dateTime);

		/// <summary>
		/// Проверяет, находится ли время в рабочих часах (9:00-18:00) по московскому времени
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns>True если время в рабочем интервале по московскому времени</returns>
		bool IsWithinWorkingHours(DateTime dateTime);

		/// <summary>
		/// Возвращает оптимальное время отправки
		/// Если текущее время рабочее - возвращает его, иначе следующее рабочее время
		/// </summary>
		/// <param name="dateTime">Желаемое время отправки</param>
		/// <returns>Оптимальное время отправки в московском времени (UTC+3)</returns>
		DateTime GetOptimalSendingTime(DateTime dateTime);

		/// <summary>
		/// Возвращает следующий рабочий день (Пн-Пт) по московскому времени
		/// </summary>
		/// <param name="dateTime">Начальная дата</param>
		/// <returns>Следующий рабочий день в московском времени (UTC+3)</returns>
		DateTime GetNextWorkingDay(DateTime dateTime);

		/// <summary>
		/// Возвращает следующее рабочее время (следующий Пн-Пт 9:00-18:00) по московскому времени
		/// </summary>
		/// <param name="dateTime">Начальное время</param>
		/// <returns>Следующее рабочее время в московском времени (UTC+3)</returns>
		DateTime GetNextWorkingTime(DateTime dateTime);
	}
}
