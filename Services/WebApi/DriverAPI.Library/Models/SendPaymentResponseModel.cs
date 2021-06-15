namespace DriverAPI.Library.Models
{
	public class SendPaymentResponseModel
	{
		public string ErrorDescription { get; set; }
		public SendPaymentResponseModelMessageStatus Status { get; set; }
		public SendPaymentResponseModelSmsPaymentStatus? PaymentStatus { get; set; }
	}
}
