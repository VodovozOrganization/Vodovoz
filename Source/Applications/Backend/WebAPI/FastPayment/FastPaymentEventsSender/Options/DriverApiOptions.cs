namespace FastPaymentEventsSender.Options
{
	/// <summary>
	/// Настройки для DriverApi
	/// </summary>
	public class DriverApiOptions
	{
		public const string Path = "DriverApiOptions";
		/// <summary>
		/// Базовый адрес
		/// </summary>
		public string BaseUrl { get; set; }
		/// <summary>
		/// Эндпойнт смены статуса оплаты быстрого платежа
		/// </summary>
		public string FastPaymentStatusChangedEndpoint { get; set; }
	}
}
