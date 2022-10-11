using System.Net;
using Vodovoz.Domain;

namespace SmsPaymentService.PaymentControllers
{
    public class TestPaymentController : IPaymentController
    {
        public SendResponse SendPayment(SmsPaymentDTO smsPayment)
        {
            return new SendResponse { HttpStatusCode = HttpStatusCode.OK, ExternalId = new System.Random().Next(1, 30000) };
        }

        public SmsPaymentStatus? GetPaymentStatus(int externalId)
        {
            return (SmsPaymentStatus)new System.Random().Next(1, 3);
        }
    }
}
