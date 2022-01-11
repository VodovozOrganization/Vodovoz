using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;

namespace Vodovoz.EntityRepositories.Payments
{
	public interface IPaymentItemsRepository
	{
		IList<PaymentItem> GetAllocatedPaymentItemsForOrder(IUnitOfWork uow, int orderId);
		decimal GetAllocatedSumForOrder(IUnitOfWork uow, int orderId);
	}
}
