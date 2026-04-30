using CustomerOrders.Contracts;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Converters
{
	public interface IExternalOrderStatusConverter
	{
		ExternalCustomerOrderStatus ConvertOnlineOrderStatus(OnlineOrderStatus onlineOrderStatus);
		ExternalCustomerOrderStatus ConvertOrderStatus(OrderStatus orderStatus);
	}
}
