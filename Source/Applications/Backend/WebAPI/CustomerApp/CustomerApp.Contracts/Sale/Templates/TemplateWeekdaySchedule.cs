using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomerApp.Contracts.Sale.Templates
{
	/// <summary>
	/// Доступные дни недели
	/// </summary>
	public class TemplateWeekdaySchedule
	{
		/// <summary>
		/// День недели
		/// </summary>
		public DayOfWeek WeekDay { get; set; }
		/// <summary>
		/// Доступность
		/// </summary>
		public bool Available { get; set; }
		/// <summary>
		/// Графики доставки
		/// </summary>
		public IEnumerable<DeliveryScheduleDto> DeliveryIntervals { get; set; }

		public static TemplateWeekdaySchedule Create(DayOfWeek weekDay, IEnumerable<DeliveryScheduleDto> deliveryIntervals = null)
		{
			return new TemplateWeekdaySchedule
			{
				WeekDay = weekDay,
				DeliveryIntervals = deliveryIntervals ?? new List<DeliveryScheduleDto>(),
				Available = deliveryIntervals != null && deliveryIntervals.Any()
			};
		}
	}
}
