using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.Models
{
	public interface IFastPaymentModel
	{
		FastPaymentStatus? GetOrderFastPaymentStatus(int orderId, int? onlineOrder = null);
	}
}
