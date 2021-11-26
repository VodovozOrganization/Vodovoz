using System;
using System.Collections.Generic;
using NHibernate;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public interface ICommonBottleAnalyticsRepository
	{
		IList<int> GetBottleAnalyticsNomenclatureIds();
		IFutureEnumerable<int> GetBottleAnalyticsNomenclatureIdsFuture(IUnitOfWork uow);
		IFutureEnumerable<NomenclatureNode> GetBottleAnalyticsNomenclaturesWithShabbyBottlesFuture(IUnitOfWork uow);

		IFutureValue<int?> GetIncomeWarehouseAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds);
		IFutureValue<int?> GetWriteoffWarehouseAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetIncomeWarehouseAssetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetWriteoffWarehouseAssetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds);

		IFutureValue<int?> GetIncomeMovementDocumentAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds);
		IFutureValue<int?> GetWriteoffMovementDocumentAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetIncomeMovementDocumentAssetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetWriteoffMovementDocumentAssetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds);

		IFutureValue<int?> GetRouteListAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetRouteListAsseetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds);
	}
}
