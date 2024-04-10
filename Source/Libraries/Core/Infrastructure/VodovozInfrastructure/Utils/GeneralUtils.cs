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

		public static DateTime GetMonthStartDateByDate(DateTime date)
		{
			var monthStartDate = new DateTime(date.Year, date.Month, 1);

			return monthStartDate;
		}

		public static DateTime GetMonthEndDateByDate(DateTime date)
		{
			var monthStartDate = GetMonthStartDateByDate(date);
			var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);

			return monthEndDate;
		}

		public static DateTime GetPreviousMonthStartDate()
		{
			var dayMonthAgo = DateTime.Today.AddMonths(-1);

			return GetMonthStartDateByDate(dayMonthAgo);
		}

		public static DateTime GetPreviousMonthEndDate()
		{
			var dayMonthAgo = DateTime.Today.AddMonths(-1);
			var monthEndDate = GetMonthEndDateByDate(dayMonthAgo);

			return monthEndDate;
		}
	}
}
