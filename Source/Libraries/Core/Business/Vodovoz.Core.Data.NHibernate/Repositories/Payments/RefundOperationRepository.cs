using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Payments
{
	public class RefundOperationRepository : IRefundOperationRepository
	{
		public RefundOperation GetSuccessfulByTransactionId(
			IUnitOfWork uow,
			string transactionId,
			OnlinePaymentSource? paymentSource)
		{
			return uow.Session.Query<RefundOperation>()
				.Where(x => x.TransactionId == transactionId
						 && x.PaymentSource == paymentSource
						 && x.IsSuccess)
				.OrderByDescending(x => x.CreatedAt)
				.FirstOrDefault();
		}

		public RefundOperation GetLastAttemptByTransactionId(
			IUnitOfWork uow,
			string transactionId,
			OnlinePaymentSource? paymentSource)
		{
			return uow.Session.Query<RefundOperation>()
				.Where(x => x.TransactionId == transactionId
						 && x.PaymentSource == paymentSource)
				.OrderByDescending(x => x.CreatedAt)
				.FirstOrDefault();
		}
	}
}
