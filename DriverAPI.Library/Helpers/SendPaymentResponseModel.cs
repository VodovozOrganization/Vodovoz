namespace DriverAPI.Library.Helpers
{
	public class SendPaymentResponseModel
	{
		public string ErrorDescription { get; set; }
		public SendPaymentResponseModelMessageStatus Status { get; set; }
		public SendPaymentResponseModelSmsPaymentStatus? PaymentStatus { get; set; }
	}
}
