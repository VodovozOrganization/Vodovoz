using Vodovoz.Domain;

namespace DriverAPI.Library.V5.Services
{
	public interface ISmsPaymentService
	{
		SmsPaymentStatus? GetOrderSmsPaymentStatus(int orderId);
	}
}
