using System;
using Vodovoz.Domain.Sale;

namespace VodovozBusiness.Extensions
{
	public static class WeekDayNameExtensions
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
	}
}
