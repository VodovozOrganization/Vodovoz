using System.Threading.Tasks;

namespace SmsPaymentService
{
    public interface ISmsPaymentStatusNotificationReciever
    {
        Task NotifyOfSmsPaymentStatusChanged(int orderId);
    }
}