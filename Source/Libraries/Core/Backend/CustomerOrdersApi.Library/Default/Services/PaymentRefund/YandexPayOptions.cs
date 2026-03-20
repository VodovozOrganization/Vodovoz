namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	/// <summary>
	/// Настройки для YandexPay
	/// </summary>
	public class YandexPayOptions
	{
		public string ApiKey { get; set; }
		public string ApiUrl { get; set; } = "https://sandbox.pay.yandex.ru/api/merchant/";
	}
}
