using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.BasicHandbooks
{
	public class DeliveryScheduleRepository : IDeliveryScheduleRepository
	{
		public QueryOver<DeliverySchedule> AllQuery()
		{
			return QueryOver.Of<DeliverySchedule>();
		}

		public QueryOver<DeliverySchedule> NotArchiveQuery()
		{
			return QueryOver.Of<DeliverySchedule>().WhereNot(ds => ds.IsArchive);
		}

		public IList<DeliverySchedule> All(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DeliverySchedule>().List<DeliverySchedule>();
		}
	}
}
