using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOrderFromOnlineOrderCreator
	{
		Order CreateOrderFromOnlineOrder(IUnitOfWork uow, Employee orderCreator, OnlineOrder onlineOrder);

		Order FillOrderFromOnlineOrder(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			PartOrderWithGoods partOrder = null,
			Employee author = null,
			bool manualCreation = false);
	}
}
