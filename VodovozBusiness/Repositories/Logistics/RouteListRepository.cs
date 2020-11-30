using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Repository.Logistics
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Logistic")]
	public static class RouteListRepository
	{
		[Obsolete]
		public static IList<RouteList> GetDriverRouteLists(IUnitOfWork uow, Employee driver, RouteListStatus status, DateTime date)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetDriverRouteLists(uow, driver, status, date);
		}

		[Obsolete]
		public static QueryOver<RouteList> GetRoutesAtDay(DateTime date)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetRoutesAtDay(date);
		}

		[Obsolete]
		public static QueryOver<RouteList> GetRoutesAtDay(DateTime date, List<int> geographicGroupsIds)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetRoutesAtDay(date, geographicGroupsIds);
		}

		[Obsolete]
		public static IList<GoodsInRouteListResult> GetGoodsAndEquipsInRL(IUnitOfWork uow, RouteList routeList, Warehouse warehouse = null)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetGoodsAndEquipsInRL(uow, routeList, warehouse);
		}

		[Obsolete]
		public static IList<GoodsLoadedListResult> AllGoodsLoaded(IUnitOfWork uow, RouteList routeList, CarLoadDocument excludeDoc = null)
		{
			return new EntityRepositories.Logistic.RouteListRepository().AllGoodsLoaded(uow, routeList, excludeDoc);
		}

		[Obsolete]
		public static List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, NomenclatureCategory[] categories, int[] excludeNomenclatureIds = null)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetReturnsToWarehouse(uow, routeListId, categories, excludeNomenclatureIds);
		}

		[Obsolete]
		public static List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, params int[] nomenclatureIds)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetReturnsToWarehouse(uow, routeListId, nomenclatureIds);
		}

		[Obsolete]
		public static IEnumerable<CarLoadDocument> GetCarLoadDocuments(IUnitOfWork uow, int routelistId)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetCarLoadDocuments(uow, routelistId);
		}
	}
}