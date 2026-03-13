namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	public static class YooKassaPaymentStatus
	{
		public const string Succeeded = "succeeded";
		public const string Pending = "pending";
		public const string WaitingForCapture = "waiting_for_capture";
		public const string Canceled = "canceled";
	}
}
