using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.V6.Services
{
	public interface IFastPaymentService
	{
		FastPaymentStatus? GetOrderFastPaymentStatus(int orderId, int? onlineOrder = null);
	}
}
