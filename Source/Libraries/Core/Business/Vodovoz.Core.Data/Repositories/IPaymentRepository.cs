using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IPaymentRepository
	{
		IEnumerable<PaymentEntity> GetOrderPayments(IUnitOfWork uow, int orderId);
	}
}
