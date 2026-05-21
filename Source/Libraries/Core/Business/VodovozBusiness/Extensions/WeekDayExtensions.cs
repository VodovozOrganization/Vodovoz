using System;
using Vodovoz.Core.Domain.Sale;

namespace VodovozBusiness.Extensions
{
	public static class WeekDayExtensions
	{
		public static WeekDayName ConvertToWeekDayName(this DayOfWeek dayOfWeek)
		{
			switch(dayOfWeek)
			{
				case DayOfWeek.Sunday:
					return WeekDayName.Sunday;
				case DayOfWeek.Monday:
					return WeekDayName.Monday;
				case DayOfWeek.Tuesday:
					return WeekDayName.Tuesday;
				case DayOfWeek.Wednesday:
					return WeekDayName.Wednesday;
				case DayOfWeek.Thursday:
					return WeekDayName.Thursday;
				case DayOfWeek.Friday:
					return WeekDayName.Friday;
				case DayOfWeek.Saturday:
					return WeekDayName.Saturday;
				default:
					throw new NotSupportedException();
			}
		}
		
		public static DayOfWeek ToDayOfWeek(this WeekDayName source)
		{
			switch(source)
			{
				case WeekDayName.Sunday:
					return DayOfWeek.Sunday;
				case WeekDayName.Monday:
					return DayOfWeek.Monday;
				case WeekDayName.Tuesday:
					return DayOfWeek.Tuesday;
				case WeekDayName.Wednesday:
					return DayOfWeek.Wednesday;
				case WeekDayName.Thursday:
					return DayOfWeek.Thursday;
				case WeekDayName.Friday:
					return DayOfWeek.Friday;
				case WeekDayName.Saturday:
					return DayOfWeek.Saturday;
				default:
					throw new NotSupportedException($"Неизвестный день недели {source}");
			}
		}
	}
}
