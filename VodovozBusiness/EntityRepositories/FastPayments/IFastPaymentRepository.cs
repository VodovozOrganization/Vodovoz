using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.EntityRepositories.FastPayments
{
	public interface IFastPaymentRepository
	{
		IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOrder(IUnitOfWork uow, int orderId);
		FastPaymentStatus? GetOrderFastPaymentStatus(IUnitOfWork uow, int orderId);
		FastPayment GetFastPaymentByTicket(IUnitOfWork uow, string ticket);
		bool FastPaymentWithTicketExists(IUnitOfWork uow, string ticket);
		IEnumerable<FastPayment> GetAllProcessingFastPayments(IUnitOfWork uow);
		FastPayment GetProcessingPaymentForOrder(IUnitOfWork uow, int orderId);
	}
}
