using CustomerOrdersApi.Library.Default.Dto.Orders;
using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V3.Factories
{
	public interface IOnlineOrderFactoryV3
	{
		OnlineOrderV1 CreateOnlineOrder(
			IUnitOfWork uow, OnlineOrderInfoDto orderInfoDto, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
	}
}
