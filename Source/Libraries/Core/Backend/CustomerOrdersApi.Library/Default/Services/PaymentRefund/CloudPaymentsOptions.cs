namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	/// <summary>
	/// Настройки для CloudPayments
	/// </summary>
	public class CloudPaymentsOptions
	{
		/// <summary>
		/// Публичный ключ для CloudPayments API
		/// </summary>
		public string PublicId { get; set; }

		/// <summary>
		/// Секретный ключ для CloudPayments API
		/// </summary>
		public string ApiSecret { get; set; }

		/// <summary>
		/// Ссылка на API CloudPayments
		/// </summary>
		public string ApiUrl { get; set; } = "https://api.cloudpayments.ru/";
	}
}
