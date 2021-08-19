using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IScheduleRestrictionRepository
	{
		QueryOver<SectorVersion> GetSectorVersion(DateTime? activationTime);
		IList<SectorVersion> GetSectorVersion(IUnitOfWork uow, DateTime? activationTime);
		IEnumerable<OrderCountResultNode> OrdersCountByDistrict(IUnitOfWork uow, DateTime date, int minBottlesInOrder);
	}
}