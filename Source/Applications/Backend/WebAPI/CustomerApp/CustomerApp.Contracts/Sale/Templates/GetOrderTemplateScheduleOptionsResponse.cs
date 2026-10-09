namespace CustomerApp.Contracts.Sale.Templates
{
	/// <summary>
	/// Настройки автозаказа
	/// </summary>
	public class GetOrderTemplateScheduleOptionsResponse
	{
		/// <summary>
		/// Еженедельные
		/// </summary>
		public WeeklyTemplateScheduleOptions Weekly { get; set; }
		/// <summary>
		/// По дням месяца
		/// </summary>
		public TemplateMonthlySchedule Monthly { get; set; }

		public static GetOrderTemplateScheduleOptionsResponse Create(
			WeeklyTemplateScheduleOptions weekly,
			TemplateMonthlySchedule monthly)
		{
			return new GetOrderTemplateScheduleOptionsResponse
			{
				Weekly = weekly,
				Monthly = monthly
			};
		}
	}
}
