using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Payments
{
	public class PaymentRepository : IPaymentRepository
	{
		public IEnumerable<PaymentEntity> GetOrderPayments(IUnitOfWork uow, int orderId)
		{
			var query =
				from paymentItem in uow.Session.Query<PaymentItemEntity>()
				join payment in uow.Session.Query<PaymentEntity>()
					on paymentItem.Payment.Id equals payment.Id
				join order in uow.Session.Query<OrderEntity>()
					on paymentItem.Order.Id equals order.Id
				where order.Id == orderId
					&& paymentItem.PaymentItemStatus != AllocationStatus.Cancelled
				select payment;
			
			return query.ToList();
		}
	}
}
