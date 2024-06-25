using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	internal sealed class PaymentFromRepository : IPaymentFromRepository
	{
		public PaymentFrom GetDuplicatePaymentFromByName(IUnitOfWork uow, int id, string name)
		{
			var query = uow.Session.QueryOver<PaymentFrom>()
				.Where(Restrictions.Conjunction()
					.Add(Restrictions.Eq(Projections.Property<PaymentFrom>(pf => pf.Name), name)));

			if(id > 0)
			{
				query.And(pf => pf.Id != id);
			}
			return query.SingleOrDefault();
		}
	}
}
