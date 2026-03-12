using CustomerOrdersApi.Library.Default.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.Factories.V3
{
	public interface IOnlineOrderFactoryV3
	{
		OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow, OnlineOrderInfoDto orderInfoDto, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
	}
}
