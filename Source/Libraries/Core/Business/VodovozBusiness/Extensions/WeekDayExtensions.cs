using System;
using CustomerOrders.Contracts.V5.Orders.Templates;
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
		
		public static WeekDayName ToWeekDayName(this ExternalWeekDayName source)
		{
			switch(source)
			{
				case ExternalWeekDayName.Monday:
					return WeekDayName.Monday;
				case ExternalWeekDayName.Tuesday:
					return WeekDayName.Tuesday;
				case ExternalWeekDayName.Wednesday:
					return WeekDayName.Wednesday;
				case ExternalWeekDayName.Thursday:
					return WeekDayName.Thursday;
				case ExternalWeekDayName.Friday:
					return WeekDayName.Friday;
				case ExternalWeekDayName.Saturday:
					return WeekDayName.Saturday;
				case ExternalWeekDayName.Sunday:
					return WeekDayName.Sunday;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение дня недели из ИПЗ");
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
		
		public static ExternalWeekDayName ToExternalWeekDayName(this WeekDayName source)
		{
			switch(source)
			{
				case WeekDayName.Monday:
					return ExternalWeekDayName.Monday;
				case WeekDayName.Tuesday:
					return ExternalWeekDayName.Tuesday;
				case WeekDayName.Wednesday:
					return ExternalWeekDayName.Wednesday;
				case WeekDayName.Thursday:
					return ExternalWeekDayName.Thursday;
				case WeekDayName.Friday:
					return ExternalWeekDayName.Friday;
				case WeekDayName.Saturday:
					return ExternalWeekDayName.Saturday;
				case WeekDayName.Sunday:
					return ExternalWeekDayName.Sunday;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение дня недели");
			}
		}
	}
}
