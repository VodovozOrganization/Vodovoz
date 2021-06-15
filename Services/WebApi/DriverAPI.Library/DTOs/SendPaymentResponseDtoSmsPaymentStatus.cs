namespace DriverAPI.Library.Models
{
	public enum SendPaymentResponseDtoSmsPaymentStatus
	{
		WaitingForPayment = 0,
		Paid = 1,
		Cancelled = 2,
		ReadyToSend = 3,
	}
}
