using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace Vodovoz.Services.Orders
{
	public interface IOrderFromOnlineOrderValidator
	{
		Result ValidateOnlineOrder(OnlineOrder onlineOrder);
	}
}
