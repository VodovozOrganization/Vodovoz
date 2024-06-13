using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Converters
{
	public interface IExternalOrderStatusConverter
	{
		ExternalOrderStatus GetExternalOrderStatus(OnlineOrder onlineOrder);
	}
}
