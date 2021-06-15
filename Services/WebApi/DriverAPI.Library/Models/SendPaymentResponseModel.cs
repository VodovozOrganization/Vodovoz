namespace DriverAPI.Library.Models
{
	public class SendPaymentResponseModel
	{
		public string ErrorDescription { get; set; }
		public SendPaymentResponseDtoMessageStatus Status { get; set; }
		public SendPaymentResponseDtoSmsPaymentStatus? PaymentStatus { get; set; }
	}
}
