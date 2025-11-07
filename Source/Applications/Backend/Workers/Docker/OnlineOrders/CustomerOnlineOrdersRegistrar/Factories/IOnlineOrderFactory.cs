using CustomerOrdersApi.Library.V4.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.Factories
{
	public interface IOnlineOrderFactory
	{
		OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow, ICreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
	}
}
