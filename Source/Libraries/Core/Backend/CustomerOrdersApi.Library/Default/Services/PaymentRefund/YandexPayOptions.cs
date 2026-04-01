namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	/// <summary>
	/// Настройки для YandexPay
	/// </summary>
	public class YandexPayOptions
	{
		/// <summary>
		/// Ключ для YandexPay API
		/// </summary>
		public string ApiKey { get; set; }

		/// <summary>
		/// Ссылка на API YandexPay
		/// </summary>
		public string ApiUrl { get; set; } = "https://sandbox.pay.yandex.ru/api/merchant/";
	}
}
