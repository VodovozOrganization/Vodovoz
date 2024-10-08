using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Converters
{
	public interface IExternalOrderStatusConverter
	{
		ExternalOrderStatus GetExternalOrderStatus(OnlineOrder onlineOrder);
	}
}
