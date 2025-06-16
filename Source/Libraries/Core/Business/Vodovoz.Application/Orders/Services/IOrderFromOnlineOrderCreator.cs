using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOrderFromOnlineOrderCreator
	{
		Order CreateOrderFromOnlineOrder(IUnitOfWork uow, Employee orderCreator, OnlineOrder onlineOrder);

		Order FillOrderFromOnlineOrder(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			Employee author = null,
			bool manualCreation = false);
	}
}
