using Vodovoz.Domain;

namespace DriverAPI.Library.DataAccess
{
	public interface ISmsPaymentModel
	{
		SmsPaymentStatus? GetOrderPaymentStatus(int orderId);
	}
}