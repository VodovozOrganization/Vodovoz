namespace DriverAPI.Library.Models
{
	public class SendPaymentResponseDto
	{
		public string ErrorDescription { get; set; }
		public SendPaymentResponseDtoMessageStatus Status { get; set; }
		public SendPaymentResponseDtoSmsPaymentStatus? PaymentStatus { get; set; }
	}
}
