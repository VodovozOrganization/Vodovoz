using Vodovoz.Domain;

namespace DriverAPI.Library.DataAccess
{
    public interface IAPISmsPaymentData
    {
        SmsPaymentStatus? GetOrderPaymentStatus(int orderId);
    }
}