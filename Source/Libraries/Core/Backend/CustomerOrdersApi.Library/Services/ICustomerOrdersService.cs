using CustomerOrdersApi.Library.Dto.Orders;

namespace CustomerOrdersApi.Library.Services
{
	public interface ICustomerOrdersService
	{
		int CreateOrderFromOnlineOrder(OnlineOrderInfoDto onlineOrderInfoDto);
	}
}
