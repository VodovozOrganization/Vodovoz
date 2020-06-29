using Vodovoz.Domain;

namespace SmsPaymentService
{
    public interface IPaymentWorker
    {
        SendResponse SendPayment(SmsPaymentDTO smsPayment);
        SmsPaymentStatus? GetPaymentStatus(int externalId);
    }
}