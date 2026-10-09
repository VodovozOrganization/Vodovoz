using System.Collections.Generic;

namespace CustomerApp.Contracts.Sale.Templates
{
	/// <summary>
	/// Еженедельные настройки для автозаказа
	/// </summary>
	public class WeeklyTemplateScheduleOptions
	{
		/// <summary>
		/// Доступность дней недели
		/// </summary>
		public IEnumerable<TemplateWeekdaySchedule> Weekdays { get; set; }
		/// <summary>
		/// Настройки по недельным повторениям
		/// </summary>
		public RepeatTemplateEveryWeeks RepeatEveryWeeks { get; set; }

		public static WeeklyTemplateScheduleOptions Create(
			RepeatTemplateEveryWeeks templateEveryWeeks,
			IEnumerable<TemplateWeekdaySchedule> weekdays
		) => new WeeklyTemplateScheduleOptions
		{
			RepeatEveryWeeks = templateEveryWeeks,
			Weekdays = weekdays
		};
	}
}
