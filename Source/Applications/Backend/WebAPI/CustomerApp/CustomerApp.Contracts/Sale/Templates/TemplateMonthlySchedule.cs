using System;
using System.Collections.Generic;

namespace CustomerApp.Contracts.Sale.Templates
{
	/// <summary>
	/// Настройки по дням месяца
	/// </summary>
	public class TemplateMonthlySchedule
	{
		/// <summary>
		/// Доступность
		/// </summary>
		public bool Available { get; set; }
		/// <summary>
		/// Информация по интервалам
		/// </summary>
		public IEnumerable<DeliveryScheduleDto> DeliveryIntervals { get; set; }
		/// <summary>
		/// Доступные настройки по конкретным дням недели
		/// </summary>
		public IEnumerable<MonthDayAvailability> MonthDays { get; set; }

		public static TemplateMonthlySchedule CreateNotAvailable() =>
			new TemplateMonthlySchedule
			{
				Available = false,
				DeliveryIntervals = Array.Empty<DeliveryScheduleDto>(),
				MonthDays = Array.Empty<MonthDayAvailability>()
			};
		
		public static TemplateMonthlySchedule CreateAvailable(
			IEnumerable<DeliveryScheduleDto> intervals,
			IEnumerable<MonthDayAvailability> days) =>
			new TemplateMonthlySchedule
			{
				Available = true,
				DeliveryIntervals = intervals,
				MonthDays = days,
			};
	}
}
