using System;
using NHibernate;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public interface IDeltaLossBottleAnalyticsRepository
	{
		IFutureEnumerable<SummaryNode> GetWriteoffLossSummaryFuture(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<SummaryNode> GetInventarizationLossSummaryFuture(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<DetailedNode> GetDriverLossByRouteListFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds);

		IFutureValue<int?> GetDriverReturnSummaryLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate, int[] nomenclatureIds);

		IFutureEnumerable<DetailedNode> GetInventorizationLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds, int? warehouseId);

		IFutureEnumerable<DetailedNode> GetWriteoffLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate, int[] nomenclatureIds,
			int? warehouseId);
	}
}
