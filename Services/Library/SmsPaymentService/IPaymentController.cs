using Vodovoz.Domain;

namespace SmsPaymentService
{
    public interface IPaymentController
    {
        SendResponse SendPayment(SmsPaymentDTO smsPayment);
        SmsPaymentStatus? GetPaymentStatus(int externalId);
    }
}