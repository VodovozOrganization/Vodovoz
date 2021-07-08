using Vodovoz.Domain;

namespace DriverAPI.Library.Models
{
	public interface ISmsPaymentModel
	{
		SmsPaymentStatus? GetOrderPaymentStatus(int orderId);
	}
}