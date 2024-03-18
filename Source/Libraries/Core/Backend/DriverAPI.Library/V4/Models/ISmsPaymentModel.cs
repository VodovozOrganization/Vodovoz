using Vodovoz.Domain;

namespace DriverAPI.Library.V4.Models
{
	public interface ISmsPaymentModel
	{
		SmsPaymentStatus? GetOrderSmsPaymentStatus(int orderId);
	}
}
