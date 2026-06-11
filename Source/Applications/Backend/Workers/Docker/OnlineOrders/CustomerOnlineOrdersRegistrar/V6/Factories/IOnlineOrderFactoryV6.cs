using CustomerOrdersApi.Library.V6.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V6.Factories
{
	public interface IOnlineOrderFactoryV6
	{
		OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow, ICreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
	}
}
