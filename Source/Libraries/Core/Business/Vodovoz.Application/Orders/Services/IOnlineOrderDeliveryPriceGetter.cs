using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOnlineOrderDeliveryPriceGetter
	{
		Result<decimal> GetDeliveryPrice(OnlineOrder onlineOrder);
	}
}
