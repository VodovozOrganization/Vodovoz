using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOnlineOrderDeliveryPriceGetter
	{
		//decimal GetDeliveryPrice(OnlineOrder onlineOrder);
		
		Result<decimal> GetDeliveryPrice(OnlineOrder onlineOrder);
	}
}
