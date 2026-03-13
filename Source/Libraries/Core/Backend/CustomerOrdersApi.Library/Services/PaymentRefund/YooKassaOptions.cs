namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	/// <summary>
	/// Настройки для ЮKassa
	/// </summary>
	public class YooKassaOptions
	{
		public string ShopId { get; set; }
		public string SecretKey { get; set; }
		public string ApiUrl { get; set; } = "https://api.yookassa.ru/v3/";
	}
}
