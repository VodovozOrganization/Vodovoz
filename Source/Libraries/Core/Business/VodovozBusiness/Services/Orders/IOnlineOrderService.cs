using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrderService
	{
		void NotifyClientOfOnlineOrderStatusChange(OnlineOrder onlineOrder, CustomerNotificationEventType eventType);
	}
}
