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
		
		public decimal GetAllocatedSumForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<PaymentItem>()
				.Where(x => x.Order.Id == orderId)
				.Select(Projections.Sum<PaymentItem>(x => x.Sum))
				.SingleOrDefault<decimal>();
		}
	}
}
