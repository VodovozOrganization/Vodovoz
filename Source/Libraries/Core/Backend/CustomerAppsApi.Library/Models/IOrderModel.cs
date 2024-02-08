using CustomerAppsApi.Library.Dto.Orders;

namespace CustomerAppsApi.Library.Models
{
	public interface IOrderModel
	{
		bool CanCounterpartyOrderPromoSetForNewClients(int counterpartyId);
		int CreateOrderFromOnlineOrder(OnlineOrderInfoDto onlineOrderInfoDto);
	}
}
