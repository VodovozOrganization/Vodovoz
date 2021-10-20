using Vodovoz.Domain;

namespace SmsPaymentService.PaymentControllers
{
    public interface IPaymentController
    {
        SendResponse SendPayment(SmsPaymentDTO smsPayment);
        SmsPaymentStatus? GetPaymentStatus(int externalId);
    }
}
