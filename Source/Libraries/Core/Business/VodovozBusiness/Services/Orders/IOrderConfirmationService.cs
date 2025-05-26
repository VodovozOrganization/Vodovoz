using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderConfirmationService
	{
		Result TryAcceptOrderCreatedByOnlineOrder(IUnitOfWork uow, Employee employee, Order order);
		void AcceptOrder(IUnitOfWork uow, Employee employee, Order order);
	}
}
