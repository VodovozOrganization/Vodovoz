using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	internal sealed class SmsPaymentRepository : ISmsPaymentRepository
	{
		public IList<SmsPayment> GetSmsPaymentsForOrder(IUnitOfWork uow, int orderId, SmsPaymentStatus[] excludeStatuses = null)
		{
			var query = uow.Session.QueryOver<SmsPayment>()
				.Where(x => x.Order.Id == orderId);

			if(excludeStatuses != null && excludeStatuses.Any())
			{
				query.WhereRestrictionOn(x => x.SmsPaymentStatus).Not.IsIn(excludeStatuses);
			}

			return query.List();
		}
	}
}
