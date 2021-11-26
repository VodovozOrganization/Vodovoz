using System;
using NHibernate;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public interface IDeltaShabbyBottleAnalyticsRepository
	{
		IFutureEnumerable<SummaryNode> GetRegradingToShabbyBottlesSummaryFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds);

		IFutureEnumerable<DetailedNode> GetRegradingToShabbyBottlesDetailedFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int? warehouseId, int[] nomenclatureIds);
	}
}
