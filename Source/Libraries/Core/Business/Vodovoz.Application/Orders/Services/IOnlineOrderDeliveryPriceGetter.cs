using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOnlineOrderDeliveryPriceGetter
	{
		decimal GetDeliveryPrice(OnlineOrder onlineOrder);
	}
}
