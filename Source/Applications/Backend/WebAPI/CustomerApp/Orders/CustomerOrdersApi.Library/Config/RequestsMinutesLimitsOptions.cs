namespace CustomerOrdersApi.Library.Config
{
	/// <summary>
	/// Ограничения по частоте обращений к эндпойнтам
	/// </summary>
	public class RequestsMinutesLimitsOptions
	{
		public const string Position = "RequestsMinutesLimits";
		
		/// <summary>
		/// Лимит обращений к эндпойнту получения причин оценки заказа(раз в n минут)
		/// </summary>
		public int OrderRatingReasonsRequestFrequencyLimit { get; set; }
	}
}
