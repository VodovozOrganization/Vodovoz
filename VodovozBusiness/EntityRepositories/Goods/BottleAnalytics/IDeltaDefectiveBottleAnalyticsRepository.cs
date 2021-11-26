using System;
using NHibernate;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public interface IDeltaDefectiveBottleAnalyticsRepository
	{
		IFutureEnumerable<SummaryNode> GetRegradingToDefectiveBottlesSummaryFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds);

		IFutureEnumerable<DetailedNode> GetRegradingToDefectiveBottlesDetailedFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int? warehouseId, int[] nomenclatureIds);
	}
}
