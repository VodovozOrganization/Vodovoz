using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public interface IOnlineOrderDeliveryPriceGetter
	{
		decimal GetDeliveryPrice(OnlineOrder onlineOrder);
	}
}
