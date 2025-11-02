using CustomerOrdersApi.Library.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.Factories
{
	public interface IOnlineOrderFactory
	{
		OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow,
			OnlineOrderInfoDto orderInfoDto,
			int fastDeliveryScheduleId,
			int selfDeliveryDiscountReasonId
		);
	}
}
