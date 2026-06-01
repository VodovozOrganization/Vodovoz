namespace CloudPaymentsApi.Library.Configs
{
	/// <summary>
	/// Настройки для CloudPayments
	/// </summary>
	public class CloudPaymentsOptions
	{
		/// <summary>
		/// Публичный ключ
		/// </summary>
		public string PublicId { get; set; }

		/// <summary>
		/// API секретный ключ
		/// </summary>
		public string ApiSecret { get; set; }

		/// <summary>
		/// Адрес доступа к API CloudPayments
		/// </summary>
		public string ApiUrl { get; set; } = "https://api.cloudpayments.ru/";
	}
}
