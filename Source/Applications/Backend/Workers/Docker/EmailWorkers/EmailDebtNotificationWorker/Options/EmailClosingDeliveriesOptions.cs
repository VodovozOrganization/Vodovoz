namespace EmailDebtNotificationWorker.Options
{
	/// <summary>
	/// Опции воркера закрытия поставок
	/// </summary>
	public class EmailClosingDeliveriesOptions
	{
		public const string SectionName = "EmailClosingDeliveriesOptions";

		/// <summary>
		/// Время запуска (час) воркера закрытия поставок
		/// </summary>
		public int StartHour { get; set; }
	}
}
