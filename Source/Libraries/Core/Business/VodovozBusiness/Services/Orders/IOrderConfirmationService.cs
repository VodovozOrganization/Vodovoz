using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Services.Logistics;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderConfirmationService
	{
		void AcceptOrder(IUnitOfWork uow, Employee employee, Order order, bool needUpdateContract = true);
		Task<Result> TryAcceptOrderCreatedByOnlineOrderAsync(IUnitOfWork uow, Employee employee, Order order, IRouteListService routeListService, CancellationToken cancellationToken);
	}
}
