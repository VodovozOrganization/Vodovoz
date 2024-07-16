using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VodovozInfrastructure.Utils
{
	public static class GeneralUtils
	{
		//new DateTime(2020, 1, 1).Range(new DateTime(2020, 1, 31)); даст перечисление дней между датами
		public static IEnumerable<DateTime> Range(this DateTime startDate, DateTime endDate)
		{
			return Enumerable.Range(0, (endDate - startDate).Days + 1).Select(d => startDate.AddDays(d));
		}

		/// <summary>
		/// Возвращает название дня на русском языке по дате
		/// Если дата на сегодня, то возвращает \"Сегодня\"
		/// Если дата на завтра, то возвращает \"Завтра\"
		/// Если дата на другой день, то возвращает название дня недели
		/// </summary>
		/// <param name="date"></param>
		/// <param name="isFirstLetterUppercase"></param>
		/// <returns></returns>
		public static string GetDayNameByDate(DateTime date, bool isFirstLetterUppercase = false)
		{
			if(date.Date == DateTime.Today)
			{
				return $"Сегодня";
			}

			if(date.Date == DateTime.Today.AddDays(1))
			{
				return $"Завтра";
			}

			var cultureInfo = CultureInfo.GetCultureInfo("ru-RU");
			var dayOfWeek = cultureInfo.DateTimeFormat.GetDayName(date.DayOfWeek);

			if(isFirstLetterUppercase)
			{
				dayOfWeek = cultureInfo.TextInfo.ToTitleCase(dayOfWeek);
			}

			return $"{dayOfWeek}";
		}

		/// <summary>
		/// Возвращает дату первого дня текущего месяца
		/// </summary>
		/// <returns></returns>
		public static DateTime GetCurrentMonthStartDate()
		{
			return GetMonthStartDateByDate(DateTime.Today);
		}

		/// <summary>
		/// Возвращает дату последнего дня текущего месяца
		/// </summary>
		/// <returns></returns>
		public static DateTime GetCurrentMonthEndDate()
		{
			return GetMonthEndDateByDate(DateTime.Today);
		}

		/// <summary>
		/// Возвращает дату первого дня месяца, следующего за текущим
		/// </summary>
		/// <returns></returns>
		public static DateTime GetNextMonthStartDate()
		{
			var dayAfterMonth = DateTime.Today.AddMonths(1);

			return GetMonthStartDateByDate(dayAfterMonth);
		}

		/// <summary>
		/// Возвращает дату последнего дня месяца, следующего за текущим
		/// </summary>
		/// <returns></returns>
		public static DateTime GetNextMonthEndDate()
		{
			var dayAfterMonth = DateTime.Today.AddMonths(1);

			return GetMonthEndDateByDate(dayAfterMonth);
		}

		/// <summary>
		/// Возвращает дату первого дня месяца, который был до текущего
		/// </summary>
		/// <returns></returns>
		public static DateTime GetPreviousMonthStartDate()
		{
			var dayMonthAgo = DateTime.Today.AddMonths(-1);

			return GetMonthStartDateByDate(dayMonthAgo);
		}

		/// <summary>
		/// Возвращает дату последнего дня месяца, который был до текущего
		/// </summary>
		/// <returns></returns>
		public static DateTime GetPreviousMonthEndDate()
		{
			var dayMonthAgo = DateTime.Today.AddMonths(-1);

			return GetMonthEndDateByDate(dayMonthAgo);
		}

		/// <summary>
		/// Возвращает дату первого дня месяца в который входит указанная дата
		/// </summary>
		/// <param name="date">Дата</param>
		/// <returns></returns>
		public static DateTime GetMonthStartDateByDate(DateTime date)
		{
			var monthStartDate = new DateTime(date.Year, date.Month, 1);

			return monthStartDate;
		}

		/// <summary>
		/// Возвращает дату последнего дня месяца в который входит указанная дата
		/// </summary>
		/// <param name="date">Дата</param>
		/// <returns></returns>
		public static DateTime GetMonthEndDateByDate(DateTime date)
		{
			var monthStartDate = GetMonthStartDateByDate(date);
			var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);

			return monthEndDate;
		}
	}
}
