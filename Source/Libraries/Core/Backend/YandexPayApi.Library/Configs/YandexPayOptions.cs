namespace YandexPayApi.Library.Configs
{
	/// <summary>
	/// Настройки для YandexPay
	/// </summary>
	public class YandexPayOptions
	{
		/// <summary>
		/// Ключ к API
		/// </summary>
		public string ApiKey { get; set; }

		/// <summary>
		/// Адрес доступа к API YandexPay
		/// </summary>
		public string ApiUrl { get; set; } = "https://sandbox.pay.yandex.ru/api/merchant/";
	}
}
