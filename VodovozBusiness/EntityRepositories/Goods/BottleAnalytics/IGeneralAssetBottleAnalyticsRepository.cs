using System;
using NHibernate;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public interface IGeneralAssetBottleAnalyticsRepository
	{
		IFutureEnumerable<NomenclatureAmountOnWarehouseNode> GetDriversLateAssetFuture(IUnitOfWork uow, DateTime date,
			int[] nomenclatureIds);

		IFutureEnumerable<NomenclatureAmountOnWarehouseNode> GetDriversOnDateAssetFuture(IUnitOfWork uow, DateTime date,
			int[] nomenclatureIds);

		IFutureEnumerable<NomenclatureAmountOnWarehouseNode> GetWarehouseIncomeAssetFuture(IUnitOfWork uow, DateTime date,
			int[] nomenclatureIds);

		IFutureEnumerable<NomenclatureAmountOnWarehouseNode> GetWarehouseWriteoffAssetFuture(IUnitOfWork uow, DateTime date,
			int[] nomenclatureIds);
	}
}
