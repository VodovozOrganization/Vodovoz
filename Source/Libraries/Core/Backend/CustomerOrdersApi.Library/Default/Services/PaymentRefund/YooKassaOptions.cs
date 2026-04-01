namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	/// <summary>
	/// Настройки для ЮKassa
	/// </summary>
	public class YooKassaOptions
	{
		/// <summary>
		/// Идентификатор магазина в ЮKassa
		/// </summary>
		public string ShopId { get; set; }

		/// <summary>
		/// Секретный ключ для ЮKassa API
		/// </summary>
		public string SecretKey { get; set; }

		/// <summary>
		/// Ссылка на API ЮKassa
		/// </summary>
		public string ApiUrl { get; set; } = "https://api.yookassa.ru/v3/";
	}
}
