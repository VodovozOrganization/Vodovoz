using CustomerAppsApi.Library.Dto.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerAppsApi.Library.Factories
{
	public interface IOnlineOrderFactory
	{
		OnlineOrder CreateOnlineOrder(OnlineOrderInfoDto orderInfoDto);
	}
}
