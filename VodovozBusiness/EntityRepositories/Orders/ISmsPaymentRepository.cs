using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface ISmsPaymentRepository
	{
		IList<SmsPayment> GetSmsPaymentsForOrder(IUnitOfWork uow, int orderId, SmsPaymentStatus[] excludeStatuses = null);
	}
}
