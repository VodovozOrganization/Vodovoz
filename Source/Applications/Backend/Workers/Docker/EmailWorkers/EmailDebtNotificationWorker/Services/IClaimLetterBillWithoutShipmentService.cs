using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace EmailDebtNotificationWorker.Services
{
	public interface IClaimLetterBillWithoutShipmentService
	{
		Task<OrderWithoutShipmentForDebt> GetOrCreateAsync(
			IUnitOfWork uow,
			Counterparty client,
			int organizationId,
			decimal debtSum,
			CancellationToken cancellationToken);
	}
}
