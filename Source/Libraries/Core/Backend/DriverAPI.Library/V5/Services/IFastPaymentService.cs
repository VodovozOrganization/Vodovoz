using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.V5.Services
{
	public interface IFastPaymentService
	{
		FastPaymentStatus? GetOrderFastPaymentStatus(int orderId, int? onlineOrder = null);
	}
}
