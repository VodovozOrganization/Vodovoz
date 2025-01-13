using Vodovoz.Domain;

namespace DriverAPI.Library.V6.Services
{
	public interface ISmsPaymentService
	{
		SmsPaymentStatus? GetOrderSmsPaymentStatus(int orderId);
	}
}
