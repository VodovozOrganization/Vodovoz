using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.BasicHandbooks
{
	public interface IDeliveryScheduleRepository
	{
		QueryOver<DeliverySchedule> AllQuery();
		QueryOver<DeliverySchedule> NotArchiveQuery();
		IList<DeliverySchedule> All(IUnitOfWork uow);

		int GetNextRoboatsId();
	}
}
