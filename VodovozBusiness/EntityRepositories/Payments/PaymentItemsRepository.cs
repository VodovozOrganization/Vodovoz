using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Payments;

namespace Vodovoz.EntityRepositories.Payments
{
	public class PaymentItemsRepository : IPaymentItemsRepository
	{
		public IList<PaymentItem> GetAllocatedPaymentItemsForOrder(IUnitOfWork uow, int orderId)
		{
			CashlessMovementOperation cashlessMovementOperationAlias = null;
			
			var paymentItems = uow.Session.QueryOver<PaymentItem>()
				.Inner.JoinAlias(pi => pi.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.Where(x => x.Order.Id == orderId)
				.List();

			return paymentItems;
		}
		
		public decimal GetAllocatedSumForOrderWithoutCurrentPayment(IUnitOfWork uow, int orderId, int paymentId)
		{
			return uow.Session.QueryOver<PaymentItem>()
				.Where(pi => pi.Order.Id == orderId)
				.And(pi => pi.Payment.Id != paymentId)
				.Select(Projections.Sum<PaymentItem>(x => x.Sum))
				.SingleOrDefault<decimal>();
		}
	}
}
