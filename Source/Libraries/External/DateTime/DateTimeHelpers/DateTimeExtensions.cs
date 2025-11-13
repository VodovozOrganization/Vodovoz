using System;
using System.Globalization;

namespace DateTimeHelpers
{
	public static class DateTimeExtensions
	{
		private static GregorianCalendar _calendar = new GregorianCalendar();

		public static int GetWeekNumber(this DateTime dateTime)
		{
			return _calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
		}

		public static DateTime LatestDayTime(this DateTime dateTime)
		{
			return dateTime.Date.AddDays(1).AddMilliseconds(-1);
		}

		public static DateTime FirstDayOfWeek(this DateTime dateTime)
		{
			switch(dateTime.DayOfWeek)
			{
				case DayOfWeek.Monday :
					return dateTime.Date;
				case DayOfWeek.Tuesday :
					return dateTime.Date.AddDays(-1);
				case DayOfWeek.Wednesday :
					return dateTime.Date.AddDays(-2);
				case DayOfWeek.Thursday :
					return dateTime.Date.AddDays(-3);
				case DayOfWeek.Friday :
					return dateTime.Date.AddDays(-4);
				case DayOfWeek.Saturday :
					return dateTime.Date.AddDays(-5);
				case DayOfWeek.Sunday:
					return dateTime.Date.AddDays(-6);
				default:
					throw new InvalidOperationException("Imposible day of week case");
			}
		}

		public static DateTime LastDayOfWeek(this DateTime dateTime)
		{
			switch(dateTime.DayOfWeek)
			{
				case DayOfWeek.Monday:
					return dateTime.Date.AddDays(6);
				case DayOfWeek.Tuesday:
					return dateTime.Date.AddDays(5);
				case DayOfWeek.Wednesday:
					return dateTime.Date.AddDays(4);
				case DayOfWeek.Thursday:
					return dateTime.Date.AddDays(3);
				case DayOfWeek.Friday:
					return dateTime.Date.AddDays(2);
				case DayOfWeek.Saturday:
					return dateTime.Date.AddDays(1);
				case DayOfWeek.Sunday:
					return dateTime.Date;
				default:
					throw new InvalidOperationException("Imposible day of week case");
			}
		}

		public static int GetQuarter(this DateTime dateTime)
		{
			if(dateTime >= new DateTime(dateTime.Year, 1, 1)
			&& dateTime < new DateTime(dateTime.Year, 4, 1))
			{
				return 1;
			}

			if(dateTime >= new DateTime(dateTime.Year, 4, 1)
			&& dateTime < new DateTime(dateTime.Year, 7, 1))
			{
				return 2;
			}

			if(dateTime >= new DateTime(dateTime.Year, 7, 1)
			&& dateTime < new DateTime(dateTime.Year, 10, 1))
			{
				return 3;
			}

			if(dateTime >= new DateTime(dateTime.Year, 10, 1)
			&& dateTime < new DateTime(dateTime.Year + 1, 1, 1))
			{
				return 4;
			}
			throw new InvalidOperationException("Imposible DateTime");
		}

		public static DateTime LastQuarterDay(this DateTime dateTime)
		{
			var quarter = dateTime.GetQuarter();

			switch(quarter)
			{
				case 1:
					return new DateTime(dateTime.Year, 3, 31);
				case 2:
					return new DateTime(dateTime.Year, 6, 30);
				case 3:
					return new DateTime(dateTime.Year, 9, 30);
				case 4:
					return new DateTime(dateTime.Year, 12, 31);
			} 
			throw new InvalidOperationException("Imposible DateTime");
		}

		public static DateTime FirstDayOfYear(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, 1, 1);
		}

		public static DateTime LastDayOfYear(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, 12, 31);
		}

		public static DateTime FirstDayOfMonth(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, 1);
		}

		public static DateTime AddWeeks(this DateTime dateTime, int weeks)
		{
			return _calendar.AddWeeks(dateTime, weeks);
		}

		public static DateTime Max(DateTime dateTime, DateTime otherDateTime)
			=> dateTime > otherDateTime ? dateTime : otherDateTime;

		public static string GetRuMonthGenetive(this DateTime dateTime)
		{
			switch(dateTime.Month)
			{
				case 1:
					return "января";
				case 2:
					return "февраля";
				case 3:
					return "марта";

				case 4:
					return "апреля";
				case 5:
					return "мая";
				case 6:
					return "июня";

				case 7:
					return "июля";
				case 8:
					return "августа";
				case 9:
					return "сентября";

				case 10:
					return "октября";
				case 11:
					return "ноября";
				case 12:
					return "декабря";

				default:
					return "-";
			}
		}
		
		public static string ToEdoShortDateString(this DateTime dateTime) => $"{dateTime:yyyy.MM.dd}";
		public static string ToEdoShortTimeString(this DateTime dateTime) => $"{dateTime:HH.mm.ss}";
	}
}
