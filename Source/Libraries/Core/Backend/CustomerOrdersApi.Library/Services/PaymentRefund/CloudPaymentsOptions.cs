namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	/// <summary>
	/// Настройки для CloudPayments
	/// </summary>
	public class CloudPaymentsOptions
	{
		public string PublicId { get; set; }
		public string ApiSecret { get; set; }
		public string ApiUrl { get; set; } = "https://api.cloudpayments.ru/";
	}
}
