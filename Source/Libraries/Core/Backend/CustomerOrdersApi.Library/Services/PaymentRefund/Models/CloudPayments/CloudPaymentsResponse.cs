namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments
{
	/// <summary>
	/// Базовый ответ от CloudPayments API
	/// </summary>
	public class CloudPaymentsResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string ErrorCode { get; set; }
		public T Model { get; set; }
	}
}
