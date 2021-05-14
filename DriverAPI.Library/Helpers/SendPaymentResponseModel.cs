namespace DriverAPI.Library.Helpers
{
    public class SendPaymentResponseModel
    {
        public string ErrorDescription { get; set; }
        public SendPaymentResponseModelMessageStatus Status { get; set; }
        public SendPaymentResponseModelSmsPaymentStatus? PaymentStatus { get; set; }
    }

    public enum SendPaymentResponseModelMessageStatus
    {
        Ok,
        Error
    }

    public enum SendPaymentResponseModelSmsPaymentStatus
    {
        WaitingForPayment = 0,
        Paid = 1,
        Cancelled = 2,
        ReadyToSend = 3,
    }
}
