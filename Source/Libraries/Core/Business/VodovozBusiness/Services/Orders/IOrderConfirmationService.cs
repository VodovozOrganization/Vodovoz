using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderConfirmationService
	{
		void AcceptOrder(IUnitOfWork uow, Employee employee, Order order, bool needUpdateContract = true);
		Task<Result> TryAcceptOrderCreatedByOnlineOrderAsync(IUnitOfWork uow, Employee employee, Order order, CancellationToken cancellationToken);
	}
}
