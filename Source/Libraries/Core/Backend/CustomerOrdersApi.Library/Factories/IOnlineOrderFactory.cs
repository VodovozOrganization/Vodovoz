using CustomerOrdersApi.Library.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Factories
{
	public interface IOnlineOrderFactory
	{
		OnlineOrder CreateOnlineOrder(IUnitOfWork uow, OnlineOrderInfoDto orderInfoDto);
	}
}
