using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Converters
{
	public interface IExternalOrderStatusConverter
	{
		ExternalOrderStatus ConvertOnlineOrderStatus(OnlineOrderStatus onlineOrderStatus);
		ExternalOrderStatus ConvertOrderStatus(OrderStatus orderStatus);
	}
}
