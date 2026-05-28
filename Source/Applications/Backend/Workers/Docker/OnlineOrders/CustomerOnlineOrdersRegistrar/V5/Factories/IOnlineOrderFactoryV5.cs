using CustomerOrdersApi.Library.V5.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V5.Factories
{
	public interface IOnlineOrderFactoryV5
	{
		OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow, ICreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
	}
}
