using CustomerOrdersApi.Library.V4.Dto.Orders;
using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V4.Factories
{
	public interface IOnlineOrderFactoryV4
	{
		OnlineOrderV1 CreateOnlineOrder(
			IUnitOfWork uow, ICreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
	}
}
