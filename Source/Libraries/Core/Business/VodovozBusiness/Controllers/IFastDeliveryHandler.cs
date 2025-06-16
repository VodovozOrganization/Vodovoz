using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Controllers
{
	public interface IFastDeliveryHandler
	{
		RouteList RouteListToAddFastDeliveryOrder { get; }
		FastDeliveryAvailabilityHistory FastDeliveryAvailabilityHistory { get; }
		Result CheckFastDelivery(IUnitOfWork uow, Order order);
		void TryAddOrderToRouteListAndNotifyDriver(IUnitOfWork uow, Order order, ICallTaskWorker callTaskWorker);
		void NotifyDriverOfFastDeliveryOrderAdded(int orderId);
	}
}
