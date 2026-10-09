using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.BasicHandbooks;

namespace Vodovoz.Infrastructure.Persistance.BasicHandbooks
{
	internal sealed class DeliveryScheduleRepository : IDeliveryScheduleRepository
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

		public INamedDomainObject Get(IUnitOfWork uow, int deliveryScheduleId)
		{
			return (
				from deliverySchedule in uow.Session.Query<DeliverySchedule>()
				where deliverySchedule.Id == deliveryScheduleId
				select new NamedDomainObjectNode
				{
					Id = deliverySchedule.Id,
					Name = deliverySchedule.Name
				})
				.FirstOrDefault();
		}

		public bool Exists(IUnitOfWork uow, int deliveryScheduleId)
		{
			return uow.Session.Query<DeliverySchedule>()
				.Any(x => x.Id == deliveryScheduleId);
		}
	}
}
