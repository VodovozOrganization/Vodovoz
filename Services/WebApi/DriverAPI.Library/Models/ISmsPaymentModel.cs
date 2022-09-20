using Vodovoz.Domain;

namespace DriverAPI.Library.Models
{
	public interface ISmsPaymentModel
	{
		SmsPaymentStatus? GetOrderSmsPaymentStatus(int orderId);
	}
}
