using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IRouteListRepository
	{
		IList<RouteList> GetDriverRouteLists(IUnitOfWork uow, Employee driver, RouteListStatus status, DateTime date);
		QueryOver<RouteList> GetRoutesAtDay(DateTime date);
		QueryOver<RouteList> GetRoutesAtDay(DateTime date, List<int> geographicGroupsIds);
		IList<GoodsInRouteListResult> GetGoodsAndEquipsInRL(IUnitOfWork uow, RouteList routeList, Warehouse warehouse);
		IList<GoodsInRouteListResult> GetGoodsInRLWithoutEquipments(IUnitOfWork uow, RouteList routeList, Warehouse warehouse);
		IList<GoodsInRouteListResult> GetEquipmentsInRL(IUnitOfWork uow, RouteList routeList, Warehouse warehouse);
		IList<GoodsLoadedListResult> AllGoodsLoaded(IUnitOfWork UoW, RouteList routeList, CarLoadDocument excludeDoc);
		List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, NomenclatureCategory[] categories, int[] excludeNomenclatureIds);
		/// <summary>
		/// Возвращает список товаров возвращенного на склад по номенклатурам
		/// </summary>
		List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, params int[] nomenclatureIds);
		IEnumerable<CarLoadDocument> GetCarLoadDocuments(IUnitOfWork uow, int routelistId);
	}
}