namespace DriverAPI.Library.Helpers
{
	public enum SendPaymentResponseModelSmsPaymentStatus
	{
		WaitingForPayment = 0,
		Paid = 1,
		Cancelled = 2,
		ReadyToSend = 3,
	}
}
