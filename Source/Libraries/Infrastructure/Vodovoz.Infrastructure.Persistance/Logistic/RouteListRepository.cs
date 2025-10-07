using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Settings;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Client;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class RouteListRepository : IRouteListRepository
	{
		private readonly ISettingsController _settingsController;
		private readonly IStockRepository _stockRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IOrganizationSettings _organizationSettings;

		public RouteListRepository(
			ISettingsController settingsController,
			IStockRepository stockRepository,
			INomenclatureSettings nomenclatureSettings,
			IOrganizationSettings organizationSettings)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
		}

		public IEnumerable<RouteList> GetDriverRouteLists(IUnitOfWork uow, int driverId, DateTime? date = null, RouteListStatus? status = null)
		{
			RouteList routeListAlias = null;

			var query = uow.Session.QueryOver(() => routeListAlias)
				.Where(() => routeListAlias.Driver.Id == driverId);

			if(date != null)
			{
				query.Where(() => routeListAlias.Date == date);
			}

			if(status != null)
			{
				query.Where(() => routeListAlias.Status == status);
			}

			return query.List();
		}

		public IEnumerable<int> GetDriverRouteListsIds(IUnitOfWork uow, Employee driver, RouteListStatus? status = null)
		{
			RouteList routeListAlias = null;

			var query = uow.Session.QueryOver(() => routeListAlias)
					  .Where(() => routeListAlias.Driver == driver);

			if(status != null)
			{
				query.Where(() => routeListAlias.Status == status);
			}

			return query.Select(x => x.Id).List<int>();
		}

		public IList<RouteList> GetRoutesAtDay(IUnitOfWork uow, DateTime dateForRouting, bool showCompleted, int[] onlyInGeographicGroup, int[] onlyWithDeliveryShifts)
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			GeoGroup routeGeographicGroupAlias = null;

			var routeListIdsQuery = QueryOver.Of(() => routeListAlias)
				.Where(() => routeListAlias.Date == dateForRouting)
				.Select(Projections.Id());

			if(!showCompleted)
			{
				routeListIdsQuery.Where(() => routeListAlias.Status == RouteListStatus.New);
			}

			if(onlyInGeographicGroup.Any())
			{
				routeListIdsQuery
					.Left.JoinAlias(() => routeListAlias.GeographicGroups, () => routeGeographicGroupAlias)
					.WhereRestrictionOn(() => routeGeographicGroupAlias.Id).IsIn(onlyInGeographicGroup);
			}

			if(onlyWithDeliveryShifts.Any())
			{
				routeListIdsQuery.WhereRestrictionOn(() => routeListAlias.Shift).IsIn(onlyWithDeliveryShifts);
			}

			var mainQuery = uow.Session.QueryOver(() => routeListAlias)
				.Where(Subqueries.WhereProperty(() => routeListAlias.Id).In(routeListIdsQuery))
				.Fetch(SelectMode.Fetch, () => routeListAlias.Driver)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Car)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Car.CarModel)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Where(Subqueries.WhereProperty(() => routeListAlias.Id).In(routeListIdsQuery))
				.Fetch(SelectMode.ChildFetch, () => routeListAlias)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Addresses)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Where(Subqueries.WhereProperty(() => routeListAlias.Id).In(routeListIdsQuery))
				.Fetch(SelectMode.ChildFetch, () => routeListAlias)
				.Fetch(SelectMode.Fetch, () => routeListAlias.GeographicGroups)
				.Future();

			uow.Session.QueryOver(() => routeListItemAlias)
				.Where(Subqueries.WhereProperty(() => routeListItemAlias.RouteList.Id).In(routeListIdsQuery))
				.Fetch(SelectMode.ChildFetch, () => routeListItemAlias)
				.Fetch(SelectMode.Fetch, () => routeListItemAlias.Order)
				.Fetch(SelectMode.Fetch, () => routeListItemAlias.Order.Contract)
				.Fetch(SelectMode.Fetch, () => routeListItemAlias.Order.DeliveryPoint)
				.Future();

			uow.Session.QueryOver(() => orderAlias)
				.JoinEntityAlias(() => routeListItemAlias, () => orderAlias.Id == routeListItemAlias.Order.Id, JoinType.LeftOuterJoin)
				.Where(Subqueries.WhereProperty(() => routeListItemAlias.RouteList.Id).In(routeListIdsQuery))
				.Fetch(SelectMode.ChildFetch, () => orderAlias)
				.Fetch(SelectMode.Fetch, () => orderAlias.OrderItems)
				.Future();

			uow.Session.QueryOver(() => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Order, () => orderAlias)
				.JoinEntityAlias(() => routeListItemAlias, () => orderAlias.Id == routeListItemAlias.Order.Id, JoinType.LeftOuterJoin)
				.Where(Subqueries.WhereProperty(() => routeListItemAlias.RouteList.Id).In(routeListIdsQuery))
				.Fetch(SelectMode.ChildFetch, () => orderItemAlias)
				.Fetch(SelectMode.Fetch, () => orderItemAlias.Nomenclature)
				.Future();


			var routeLists = mainQuery.ToList();

			return routeLists;
		}

		public QueryOver<RouteList> GetRoutesAtDay(DateTime date, List<int> geographicGroupsIds, bool onlyNonPrinted)
		{
			GeoGroup geographicGroupAlias = null;
			RouteList routeListAlias = null;
			DocumentPrintHistory printHistoryAlias = null;

			var query = QueryOver.Of(() => routeListAlias)
				.Where(x => x.Date == date);

			if(onlyNonPrinted)
			{
				query.Left.JoinAlias(() => routeListAlias.PrintsHistory, () => printHistoryAlias)
					.WhereRestrictionOn(() => printHistoryAlias.Id).IsNull();
			}

			if(geographicGroupsIds.Any())
			{
				var routeListsWithGeoGroupsSubquery = QueryOver.Of<RouteList>()
					.Left.JoinAlias(r => r.GeographicGroups, () => geographicGroupAlias)
					.Where(() => geographicGroupAlias.Id.IsIn(geographicGroupsIds))
					.Select(r => r.Id);
				query.WithSubquery.WhereProperty(r => r.Id).In(routeListsWithGeoGroupsSubquery);
			}

			return query;
		}

		public IList<GoodsInRouteListResultWithSpecialRequirements> GetGoodsAndEquipsInRLWithSpecialRequirements(
			IUnitOfWork uow,
			RouteList routeList,
			ISubdivisionRepository subdivisionRepository = null,
			Warehouse warehouse = null)
		{
			if(subdivisionRepository == null && warehouse != null)
			{
				throw new ArgumentNullException(nameof(subdivisionRepository));
			}

			List<GoodsInRouteListResultWithSpecialRequirements> result = new List<GoodsInRouteListResultWithSpecialRequirements>();

			GoodsInRouteListResultWithSpecialRequirements terminal = null;

			if(warehouse != null)
			{
				var cashSubdivisions = subdivisionRepository.GetCashSubdivisions(uow);
				if(cashSubdivisions.Any(x => x.Id == warehouse.OwningSubdivisionId))
				{
					terminal = GetTerminalInRLWithSpecialRequirements(uow, routeList, warehouse);
					if(routeList.AdditionalLoadingDocument != null)
					{
						result.AddRange(GetGoodsInRLWithoutEquipmentsWithSpecialRequirements(uow, routeList).ToList());
						result.AddRange(GetEquipmentsInRLWithSpecialRequirements(uow, routeList).ToList());
					}
				}
				else
				{
					result.AddRange(GetGoodsInRLWithoutEquipmentsWithSpecialRequirements(uow, routeList).ToList());
					result.AddRange(GetEquipmentsInRLWithSpecialRequirements(uow, routeList).ToList());
				}
			}
			else
			{
				result.AddRange(GetGoodsInRLWithoutEquipmentsWithSpecialRequirements(uow, routeList).ToList());
				result.AddRange(GetEquipmentsInRLWithSpecialRequirements(uow, routeList).ToList());

				terminal = GetTerminalInRLWithSpecialRequirements(uow, routeList);
			}

			if(terminal != null)
			{
				result.Add(terminal);
			}

			if(routeList.AdditionalLoadingDocument != null)
			{
				result.AddRange(
					routeList.AdditionalLoadingDocument.Items.Select(x => new GoodsInRouteListResultWithSpecialRequirements
					{
						NomenclatureId = x.Nomenclature.Id,
						NomenclatureName = x.Nomenclature.Name,
						Amount = x.Amount
					})
				);
			}

			return result
				.GroupBy(x => new
				{
					x.NomenclatureId,
					x.ExpireDatePercent,
					x.OwnType,
					x.OrderId
				}
				).Select(list => new GoodsInRouteListResultWithSpecialRequirements()
				{
					NomenclatureName = list.First().NomenclatureName,
					NomenclatureId = list.Key.NomenclatureId,
					OwnType = list.Key.OwnType,
					ExpireDatePercent = list.Key.ExpireDatePercent,
					Amount = list.Sum(x => x.Amount),
					OrderId = list.Key.OrderId,
					IsNeedIndividualSetOnLoad = list.FirstOrDefault().IsNeedIndividualSetOnLoad,
				}
				).ToList();
		}

		public IList<GoodsInRouteListResult> GetGoodsAndEquipsInRL(
			IUnitOfWork uow,
			RouteList routeList,
			ISubdivisionRepository subdivisionRepository = null,
			Warehouse warehouse = null)
		{
			if(subdivisionRepository == null && warehouse != null)
			{
				throw new ArgumentNullException(nameof(subdivisionRepository));
			}

			List<GoodsInRouteListResult> result = new List<GoodsInRouteListResult>();

			if(warehouse != null)
			{
				var cashSubdivisions = subdivisionRepository.GetCashSubdivisions(uow);
				if(cashSubdivisions.Any(x => x.Id == warehouse.OwningSubdivisionId))
				{
					var terminal = GetTerminalInRL(uow, routeList, warehouse);
					if(terminal != null)
					{
						result.Add(terminal);
					}
				}
				else
				{
					result.AddRange(GetGoodsInRLWithoutEquipments(uow, routeList).ToList());
					result.AddRange(GetEquipmentsInRL(uow, routeList).ToList());
				}
			}
			else
			{
				result.AddRange(GetGoodsInRLWithoutEquipments(uow, routeList).ToList());
				result.AddRange(GetEquipmentsInRL(uow, routeList).ToList());

				var terminal = GetTerminalInRL(uow, routeList);
				if(terminal != null)
				{
					result.Add(terminal);
				}
			}

			if(routeList.AdditionalLoadingDocument != null)
			{
				var fastDeliveryOrdersItemsInRL = GetFastDeliveryOrdersItemsInRL(uow, routeList.Id);
				foreach(var additionalItem in routeList.AdditionalLoadingDocument.Items)
				{
					var fastDeliveryItem = fastDeliveryOrdersItemsInRL.FirstOrDefault(x => x.NomenclatureId == additionalItem.Nomenclature.Id);
					result.Add(new GoodsInRouteListResult
					{
						NomenclatureId = additionalItem.Nomenclature.Id,
						Amount = additionalItem.Amount - (fastDeliveryItem?.Amount ?? 0)
					});
				}
			}

			return result
				.GroupBy(x => x.NomenclatureId, x => x.Amount)
				.Select(x => new GoodsInRouteListResult
				{
					NomenclatureId = x.Key,
					Amount = x.Sum()
				}
				)
				.ToList();
		}

		public IList<GoodsInRouteListResult> GetGoodsInRLWithoutEquipments(IUnitOfWork uow, RouteList routeList)
		{
			GoodsInRouteListResult resultAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemsAlias = null;
			Nomenclature orderItemNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of(() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<RouteListItem>()
				.Where(r => r.RouteList.Id == routeList.Id)
				.Where(r => r.TransferedTo == null &&
							(!r.WasTransfered || r.AddressTransferType.IsIn(AddressTransferTypesWithoutTransferFromHandToHand)))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderitemsQuery = uow.Session.QueryOver(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => orderItemNomenclatureAlias)
				.Where(() => orderItemNomenclatureAlias.Category.IsIn(Nomenclature.GetCategoriesForShipment()));

			return orderitemsQuery.SelectList(list => list
				.SelectGroup(() => orderItemNomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.SelectSum(() => orderItemsAlias.Count).WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();
		}

		public IList<GoodsInRouteListResultWithSpecialRequirements> GetGoodsInRLWithoutEquipmentsWithSpecialRequirements(IUnitOfWork uow, RouteList routeList)
		{
			GoodsInRouteListResultWithSpecialRequirements resultAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemsAlias = null;
			Counterparty counterpartyAlias = null;
			Nomenclature orderItemNomenclatureAlias = null;
			CounterpartyContract contractAlias = null;
			Organization organizationAlias = null;
			CounterpartyEdoAccount defaultEdoAccountAlias = null;

			var ordersQuery = QueryOver.Of(() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<RouteListItem>()
				.Where(r => r.RouteList.Id == routeList.Id)
				.Where(r => r.TransferedTo == null &&
					(!r.WasTransfered || r.AddressTransferType.IsIn(AddressTransferTypesWithoutTransferFromHandToHand)))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);
			
			var isNeedIndividualSetOnLoadSubquery = QueryOver.Of(() => orderAlias)
				.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinAlias(() => orderAlias.Contract, () => contractAlias)
				.JoinAlias(() => contractAlias.Organization, () => organizationAlias)
				.JoinAlias(() => counterpartyAlias.CounterpartyEdoAccounts,
					() => defaultEdoAccountAlias,
					JoinType.LeftOuterJoin,
					Restrictions.Where(() => defaultEdoAccountAlias.OrganizationId == organizationAlias.Id
						&& defaultEdoAccountAlias.IsDefault))
				.Where(() => orderAlias.Id == orderItemsAlias.Order.Id)
				.Select(Projections.Conditional(
					Restrictions.Disjunction()
						.Add(
							Restrictions.Conjunction()
								.Add(Restrictions.Eq(Projections.Property(() => orderAlias.PaymentType), PaymentType.Cashless))
								.Add(Restrictions.Eq(Projections.Property(() => counterpartyAlias.OrderStatusForSendingUpd), OrderStatusForSendingUpd.EnRoute))
								.Add(Restrictions.Eq(Projections.Property(() => defaultEdoAccountAlias.ConsentForEdoStatus), ConsentForEdoStatus.Agree))
						)
						.Add(
							Restrictions.Conjunction()
								.Add(Restrictions.Eq(Projections.Property(() => orderAlias.PaymentType), PaymentType.Cashless))
								.Add(Restrictions.Eq(Projections.Property(() => counterpartyAlias.OrderStatusForSendingUpd), OrderStatusForSendingUpd.EnRoute))
								.Add(Restrictions.Eq(Projections.Property(() => counterpartyAlias.ReasonForLeaving), ReasonForLeaving.Tender))
						),
					Projections.Constant(true),
					Projections.Constant(false)
				));

			var orderIdProjection = Projections.Conditional(
				Restrictions.Eq(Projections.SubQuery(isNeedIndividualSetOnLoadSubquery), true),
				Projections.Property(() => orderAlias.Id),
				Projections.Constant(0));

			var orderitemsQuery = uow.Session.QueryOver(() => orderItemsAlias)
					.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
					.JoinAlias(() => orderItemsAlias.Nomenclature, () => orderItemNomenclatureAlias)
					.JoinAlias(() => orderItemsAlias.Order, () => orderAlias)
					.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
					.Where(() => orderItemNomenclatureAlias.Category.IsIn(Nomenclature.GetCategoriesForShipment()));

			var itemsByOrders = orderitemsQuery.SelectList(list => list
				.Select(
					Projections.GroupProperty(
						Projections.Conditional(
							Restrictions.And(
								Restrictions.Where(() => counterpartyAlias.SpecialExpireDatePercentCheck),
								Restrictions.Where(() => orderItemNomenclatureAlias.Category == NomenclatureCategory.water)
							),
							Projections.SqlFunction(
								new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT(?1, ' >', ?2, '% срока годности')"),
								NHibernateUtil.String,
								Projections.Property(() => orderItemNomenclatureAlias.Name),
								Projections.Cast(NHibernateUtil.String, Projections.Property(() => counterpartyAlias.SpecialExpireDatePercent))
							),
							Projections.Property(() => orderItemNomenclatureAlias.Name)
						)
					)
				).WithAlias(() => resultAlias.NomenclatureName)
				.Select(Projections.GroupProperty(orderIdProjection)).WithAlias(() => resultAlias.OrderId)
				.Select(
					Projections.Conditional(
						Restrictions.And(
							Restrictions.Where(() => counterpartyAlias.SpecialExpireDatePercentCheck),
							Restrictions.Where(() => orderItemNomenclatureAlias.Category == NomenclatureCategory.water)
						),
						Projections.Property(() => counterpartyAlias.SpecialExpireDatePercent),
						Projections.Constant(null, NHibernateUtil.Decimal)
					)
				).WithAlias(() => resultAlias.ExpireDatePercent)
				.Select(() => orderItemNomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.SelectSum(() => orderItemsAlias.Count).WithAlias(() => resultAlias.Amount)
				.SelectSubQuery(isNeedIndividualSetOnLoadSubquery).WithAlias(() => resultAlias.IsNeedIndividualSetOnLoad))
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResultWithSpecialRequirements>())
				.List<GoodsInRouteListResultWithSpecialRequirements>();

			foreach(var item in itemsByOrders)
			{
				if(item.OrderId == 0)
				{
					item.OrderId = null;
				}
			}

			return itemsByOrders;
		}

		public IList<GoodsInRouteListResultWithSpecialRequirements> GetEquipmentsInRLWithSpecialRequirements(IUnitOfWork uow, RouteList routeList)
		{
			GoodsInRouteListResultWithSpecialRequirements resultAlias = null;
			VodovozOrder orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature orderEquipmentNomenclatureAlias = null;

			//Выбирается список Id заказов находящихся в МЛ
			var ordersQuery = QueryOver.Of(() => orderAlias);
			var routeListItemsSubQuery = QueryOver.Of<RouteListItem>()
				.Where(r => r.RouteList.Id == routeList.Id)
				.Where(r => r.TransferedTo == null &&
					(!r.WasTransfered || r.AddressTransferType.IsIn(AddressTransferTypesWithoutTransferFromHandToHand)))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderEquipmentsQuery = uow.Session.QueryOver(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => orderEquipmentNomenclatureAlias);

			return orderEquipmentsQuery
				.SelectList(list => list
					.SelectGroup(() => orderEquipmentNomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.SelectGroup(() => orderEquipmentAlias.OwnType).WithAlias(() => resultAlias.OwnType)
					.Select(Projections.Sum(
						Projections.Cast(NHibernateUtil.Decimal, Projections.Property(() => orderEquipmentAlias.Count))
					)).WithAlias(() => resultAlias.Amount)
					.Select(() => orderEquipmentNomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => orderEquipmentAlias.OwnType).WithAlias(() => resultAlias.OwnType)
				)
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResultWithSpecialRequirements>())
				.List<GoodsInRouteListResultWithSpecialRequirements>();
		}

		public IList<GoodsInRouteListResult> GetEquipmentsInRL(IUnitOfWork uow, RouteList routeList)
		{
			GoodsInRouteListResult resultAlias = null;
			VodovozOrder orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature orderEquipmentNomenclatureAlias = null;

			//Выбирается список Id заказов находящихся в МЛ
			var ordersQuery = QueryOver.Of(() => orderAlias);
			var routeListItemsSubQuery = QueryOver.Of<RouteListItem>()
				.Where(r => r.RouteList.Id == routeList.Id)
				.Where(r => r.TransferedTo == null &&
					(!r.WasTransfered || r.AddressTransferType.IsIn(AddressTransferTypesWithoutTransferFromHandToHand)))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderEquipmentsQuery = uow.Session.QueryOver(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => orderEquipmentNomenclatureAlias);

			return orderEquipmentsQuery
				.SelectList(list => list
				   .SelectGroup(() => orderEquipmentNomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
							.Select(Projections.Sum(
					   Projections.Cast(NHibernateUtil.Decimal, Projections.Property(() => orderEquipmentAlias.Count)))).WithAlias(() => resultAlias.Amount)
				)
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();
		}

		public GoodsInRouteListResult GetTerminalInRL(IUnitOfWork uow, RouteList routeList, Warehouse warehouse = null)
		{
			CarLoadDocumentItem carLoadDocumentItemAlias = null;

			var terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
			var needTerminal = routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

			var loadedTerminal = uow.Session.QueryOver<CarLoadDocument>()
									.JoinAlias(x => x.Items, () => carLoadDocumentItemAlias)
									.Where(() => carLoadDocumentItemAlias.Nomenclature.Id == terminalId)
									.And(x => x.RouteList.Id == routeList.Id)
									.List();

			if(GetLastTerminalDocumentForEmployee(uow, routeList.Driver) is DriverAttachedTerminalGiveoutDocument)
			{
				return null;
			}

			if(needTerminal || loadedTerminal.Any())
			{
				var terminal = uow.GetById<Nomenclature>(terminalId);
				int amount = 1;

				if(warehouse == null)
				{
					return new GoodsInRouteListResult
					{
						NomenclatureId = terminalId,
						Amount = amount
					};
				}

				if(_stockRepository.NomenclatureInStock(uow, new int[] { terminal.Id }, new[] { warehouse.Id }).Any())
				{
					return new GoodsInRouteListResult
					{
						NomenclatureId = terminalId,
						Amount = amount
					};
				}
			}

			return null;
		}

		public IList<GoodsInRouteListResult> GetFastDeliveryOrdersItemsInRL(IUnitOfWork uow, int routeListId, RouteListItemStatus[] excludeAddressStatuses = null)
		{
			GoodsInRouteListResult resultAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemsAlias = null;
			RouteListItem routeListItemAlias = null;
			OrderEquipment orderEquipmentAlias = null;

			List<GoodsInRouteListResult> goodsInRouteListResults = new List<GoodsInRouteListResult>();

			var items = uow.Session.QueryOver(() => orderItemsAlias)
				.JoinAlias(() => orderItemsAlias.Order, () => orderAlias)
				.JoinEntityAlias(() => routeListItemAlias, () => routeListItemAlias.Order.Id == orderAlias.Id)
				.Where(() => orderAlias.IsFastDelivery)
				.And(() => routeListItemAlias.RouteList.Id == routeListId)
				.And(() => routeListItemAlias.TransferedTo == null
						   && (!routeListItemAlias.WasTransfered || routeListItemAlias.AddressTransferType != AddressTransferType.NeedToReload))
				.SelectList(list => list
					.SelectGroup(() => orderItemsAlias.Nomenclature.Id).WithAlias(() => resultAlias.NomenclatureId)
					.SelectSum(() => orderItemsAlias.Count).WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>());

			var equipment = uow.Session.QueryOver(() => orderEquipmentAlias)
				.JoinAlias(() => orderEquipmentAlias.Order, () => orderAlias)
				.JoinEntityAlias(() => routeListItemAlias, () => routeListItemAlias.Order.Id == orderAlias.Id)
				.Where(() => orderAlias.IsFastDelivery)
				.And(() => routeListItemAlias.TransferedTo == null
						   && (!routeListItemAlias.WasTransfered || routeListItemAlias.AddressTransferType != AddressTransferType.NeedToReload))
				.And(() => routeListItemAlias.RouteList.Id == routeListId)
				.And(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.SelectList(list => list
					.SelectGroup(() => orderEquipmentAlias.Nomenclature.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(Projections.Cast(NHibernateUtil.Decimal, Projections.Sum(Projections.Property(() => orderEquipmentAlias.Count)))).WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>());

			if(excludeAddressStatuses != null)
			{
				items.Where(() => !routeListItemAlias.Status.IsIn(excludeAddressStatuses));
				equipment.Where(() => !routeListItemAlias.Status.IsIn(excludeAddressStatuses));
			}

			goodsInRouteListResults.AddRange(
				items.List<GoodsInRouteListResult>()
					.Union(equipment.List<GoodsInRouteListResult>()));

			return goodsInRouteListResults;
		}

		public DriverAttachedTerminalDocumentBase GetLastTerminalDocumentForEmployee(IUnitOfWork uow, Employee employee)
		{
			DriverAttachedTerminalDocumentBase docAlias = null;

			return uow.Session.QueryOver(() => docAlias)
				.Where(doc => doc.Driver == employee)
				.OrderBy(doc => doc.CreationDate).Desc.Take(1)
				.SingleOrDefault();
		}

		public bool IsTerminalRequired(IUnitOfWork uow, int routeListId)
		{
			CarLoadDocumentItem carLoadDocumentItemAlias = null;

			var terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
			var routeList = uow.Query<RouteList>().Where(x => x.Id == routeListId).SingleOrDefault();
			var anyAddressesRequireTerminal = routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

			var giveoutDoc = GetLastTerminalDocumentForEmployee(uow, routeList.Driver) as DriverAttachedTerminalGiveoutDocument;

			var loadedTerminal = uow.Session.QueryOver<CarLoadDocument>()
									.JoinAlias(x => x.Items, () => carLoadDocumentItemAlias)
									.Where(() => carLoadDocumentItemAlias.Nomenclature.Id == terminalId)
									.And(() => carLoadDocumentItemAlias.Amount > 0)
									.And(x => x.RouteList.Id == routeList.Id)
									.List();

			return anyAddressesRequireTerminal && !loadedTerminal.Any() && giveoutDoc == null;
		}

		public GoodsInRouteListResultWithSpecialRequirements GetTerminalInRLWithSpecialRequirements(IUnitOfWork uow, RouteList routeList, Warehouse warehouse = null)
		{
			CarLoadDocumentItem carLoadDocumentItemAlias = null;

			if(GetLastTerminalDocumentForEmployee(uow, routeList.Driver) is DriverAttachedTerminalGiveoutDocument giveoutDoc)
			{//если водителю был привязан терминал, то ему не надо его грузить
				return null;
			}

			if(GetSelfDriverTerminalTransferDocument(uow, routeList.Driver, routeList) != null)
			{//если терминал был перенесён на вторую ходку, то не надо его грузить
				return null;
			}

			var transferedCount = TerminalTransferedCountToRouteList(uow, routeList);

			var terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
			var needTerminal = routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal) && transferedCount == 0;

			var loadedTerminal = uow.Session.QueryOver<CarLoadDocument>()
									.JoinAlias(x => x.Items, () => carLoadDocumentItemAlias)
									.Where(() => carLoadDocumentItemAlias.Nomenclature.Id == terminalId)
									.And(x => x.RouteList.Id == routeList.Id)
									.List();

			if(needTerminal || loadedTerminal.Any())
			{
				var terminal = uow.GetById<Nomenclature>(terminalId);
				int amount = 1;

				if(warehouse == null)
				{
					return new GoodsInRouteListResultWithSpecialRequirements
					{
						NomenclatureName = terminal.Name,
						NomenclatureId = terminalId,
						Amount = amount
					};
				}

				if(_stockRepository.NomenclatureInStock(uow, new int[] { terminal.Id }, new[] { warehouse.Id }).Any())
				{
					return new GoodsInRouteListResultWithSpecialRequirements
					{
						NomenclatureName = terminal.Name,
						NomenclatureId = terminalId,
						Amount = amount
					};
				}
			}

			return null;
		}

		public IList<GoodsInRouteListResultToDivide> AllGoodsLoadedDivided(IUnitOfWork uow, RouteList routeList, CarLoadDocument excludeDoc = null)
		{
			CarLoadDocument docAlias = null;
			CarLoadDocumentItem docItemsAlias = null;
			GoodsInRouteListResultToDivide inCarLoads = null;

			var loadedQuery = uow.Session.QueryOver(() => docAlias)
				.Where(d => d.RouteList.Id == routeList.Id);

			if(excludeDoc != null)
			{
				loadedQuery.Where(d => d.Id != excludeDoc.Id);
			}

			var loadedlist = loadedQuery
				.JoinAlias(d => d.Items, () => docItemsAlias)
				.SelectList(list => list
					.SelectGroup(() => docItemsAlias.Nomenclature.Id).WithAlias(() => inCarLoads.NomenclatureId)
					.SelectSum(() => docItemsAlias.Amount).WithAlias(() => inCarLoads.Amount)
					.SelectGroup(() => docItemsAlias.ExpireDatePercent).WithAlias(() => inCarLoads.ExpireDatePercent)
					.SelectGroup(() => docItemsAlias.OwnType).WithAlias(() => inCarLoads.OwnType)
					.SelectGroup(() => docItemsAlias.OrderId).WithAlias(() => inCarLoads.OrderId)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResultToDivide>())
				.List<GoodsInRouteListResultToDivide>();
			return loadedlist;
		}

		public IList<GoodsInRouteListResult> AllGoodsLoaded(IUnitOfWork uow, RouteList routeList, CarLoadDocument excludeDoc = null)
		{
			CarLoadDocument docAlias = null;
			CarLoadDocumentItem docItemsAlias = null;
			GoodsInRouteListResult inCarLoads = null;

			var loadedQuery = uow.Session.QueryOver(() => docAlias)
				.Where(d => d.RouteList.Id == routeList.Id);

			if(excludeDoc != null)
				loadedQuery.Where(d => d.Id != excludeDoc.Id);

			var loadedlist = loadedQuery
				.JoinAlias(d => d.Items, () => docItemsAlias)
				.SelectList(list => list
					.SelectGroup(() => docItemsAlias.Nomenclature.Id).WithAlias(() => inCarLoads.NomenclatureId)
					.SelectSum(() => docItemsAlias.Amount).WithAlias(() => inCarLoads.Amount)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();
			return loadedlist;
		}

		public IEnumerable<GoodsInRouteListResult> AllGoodsDelivered(IUnitOfWork uow, RouteList routeList, DeliveryDirection? deliveryDirection = null)
		{
			if(routeList == null) throw new ArgumentNullException(nameof(routeList));

			IList<GoodsInRouteListResult> result = new List<GoodsInRouteListResult>();
			if(!routeList.Addresses.Any())
			{
				return result;
			}

			DeliveryDocument docAlias = null;
			DeliveryDocumentItem docItemsAlias = null;
			GoodsInRouteListResult resultNodeAlias = null;

			var query = uow.Session.QueryOver(() => docAlias)
				.Inner.JoinAlias(d => d.Items, () => docItemsAlias)
				.WhereRestrictionOn(d => d.RouteListItem.Id).IsIn(routeList.Addresses.Select(x => x.Id).ToArray());

			if(deliveryDirection != null)
			{
				query.Where(() => docItemsAlias.Direction == deliveryDirection);
			}

			result = query
				.SelectList(list => list
					.SelectGroup(() => docItemsAlias.Nomenclature.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
					.SelectSum(() => docItemsAlias.Amount).WithAlias(() => resultNodeAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();

			return result;
		}

		public IEnumerable<GoodsInRouteListResult> GetActualGoodsForShipment(IUnitOfWork uow, int routeListId)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemsAlias = null;
			RouteListItem addressAlias = null;
			GoodsInRouteListResult resultNodeAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => addressAlias)
				.JoinAlias(() => addressAlias.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => addressAlias.RouteList.Id == routeListId)
				.WhereRestrictionOn(() => nomenclatureAlias.Category).IsIn(Nomenclature.GetCategoriesForShipment())
				.WhereRestrictionOn(() => addressAlias.Status).Not.IsIn(new[] { RouteListItemStatus.Transfered });

			var result = query
				.SelectList(list => list
					.SelectGroup(() => orderItemsAlias.Nomenclature.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
					.SelectSum(() => orderItemsAlias.ActualCount).WithAlias(() => resultNodeAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();

			return result;
		}

		public IEnumerable<GoodsInRouteListResult> GetActualEquipmentForShipment(IUnitOfWork uow, int routeListId, Direction direction)
		{
			VodovozOrder orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			RouteListItem addressAlias = null;
			GoodsInRouteListResult resultNodeAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => addressAlias)
				.JoinAlias(() => addressAlias.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => addressAlias.RouteList.Id == routeListId)
				.WhereRestrictionOn(() => nomenclatureAlias.Category).IsIn(Nomenclature.GetCategoriesForShipment())
				.WhereRestrictionOn(() => addressAlias.Status).Not.IsIn(new[] { RouteListItemStatus.Transfered })
				.And(() => orderEquipmentAlias.Direction == direction);

			var result = query
				.SelectList(list => list
					.SelectGroup(() => orderEquipmentAlias.Nomenclature.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
					.Select(Projections.Sum(
						Projections.Cast(NHibernateUtil.Decimal, Projections.Property(() => orderEquipmentAlias.ActualCount))))
					.WithAlias(() => resultNodeAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();

			return result;
		}

		public bool HasFreeBalanceForOrder(IUnitOfWork uow, VodovozOrder order, RouteList routeListTo)
		{
			GoodsInRouteListResult resultAlias = null;

			var freeBalance = routeListTo.ObservableDeliveryFreeBalanceOperations
				.GroupBy(o => o.Nomenclature)
				.Select(list => new GoodsInRouteListResult
				{
					NomenclatureId = list.First().Nomenclature.Id,
					Amount = list.Sum(x => x.Amount)
				});

			var nomenclaturesToDeliver = order.GetAllGoodsToDeliver();

			return nomenclaturesToDeliver.All(item =>
				item.Amount <= freeBalance.SingleOrDefault(fb => fb.NomenclatureId == item.NomenclatureId)?.Amount);
		}

		public bool IsOrderNeedIndividualSetOnLoad(IUnitOfWork uow, int orderId)
		{
			return uow.Session.Query<CarLoadDocumentItem>()
				.Where(d => d.OrderId == orderId)
				.Any(d => d.IsIndividualSetForOrder);
		}

		public IEnumerable<GoodsInRouteListResult> AllGoodsDelivered(IEnumerable<DeliveryDocument> deliveryDocuments)
		{
			return deliveryDocuments.SelectMany(d => d.Items.Where(x => x.Direction == DeliveryDirection.ToClient))
				.GroupBy(i => i.Nomenclature.Id,
					(nomenclatureId, items) =>
						new GoodsInRouteListResult
						{
							NomenclatureId = nomenclatureId,
							Amount = items.Sum(x => x.Amount)
						});
		}

		public IEnumerable<GoodsInRouteListResult> AllGoodsReceivedFromClient(IEnumerable<DeliveryDocument> deliveryDocuments)
		{
			return deliveryDocuments.SelectMany(d => d.Items.Where(x => x.Direction == DeliveryDirection.FromClient))
				.GroupBy(i => i.Nomenclature.Id,
					(nomenclatureId, items) =>
						new GoodsInRouteListResult
						{
							NomenclatureId = nomenclatureId,
							Amount = items.Sum(x => x.Amount)
						});
		}

		public decimal TerminalTransferedCountToRouteList(IUnitOfWork unitOfWork, RouteList routeList)
		{
			if(unitOfWork == null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			if(routeList == null)
			{
				throw new ArgumentNullException(nameof(routeList));
			}

			var termanalTransferDocumentItems = unitOfWork.Session.Query<DriverTerminalTransferDocumentBase>()
				.Where(dttd => dttd.RouteListTo.Id == routeList.Id
							|| dttd.RouteListFrom.Id == routeList.Id)
				.ToList();

			if(termanalTransferDocumentItems.Any())
			{
				return termanalTransferDocumentItems.Sum(dttd =>
					dttd.RouteListTo.Id == routeList.Id ? 1 :
					dttd.RouteListFrom.Id == routeList.Id ? -1 : 0);
			}

			return 0;
		}

		public IList<DocumentPrintHistory> GetPrintsHistory(IUnitOfWork uow, RouteList routeList)
		{
			var types = new[] { PrintedDocumentType.RouteList, PrintedDocumentType.ClosedRouteList };
			return uow.Session.Query<DocumentPrintHistory>()
				.Where(dph => types.Contains(dph.DocumentType) && dph.RouteList.Id == routeList.Id).ToList();
		}

		public IEnumerable<GoodsInRouteListResult> AllGoodsTransferredToAnotherDrivers(IUnitOfWork uow, RouteList routeList,
			NomenclatureCategory[] categories = null, AddressTransferType? addressTransferType = null)
		{
			if(routeList == null) throw new ArgumentNullException(nameof(routeList));

			AddressTransferDocument transferDocAlias = null;
			AddressTransferDocumentItem transferDocItemAlias = null;
			DriverNomenclatureTransferItem driverTransferDocItemAlias = null;
			GoodsInRouteListResult resultNodeAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => transferDocAlias)
				.JoinAlias(() => transferDocAlias.AddressTransferDocumentItems, () => transferDocItemAlias)
				.JoinAlias(() => transferDocItemAlias.DriverNomenclatureTransferDocumentItems, () => driverTransferDocItemAlias)
				.Where(() => transferDocAlias.RouteListFrom.Id == routeList.Id);

			if(addressTransferType.HasValue)
			{
				query.Where(() => transferDocItemAlias.AddressTransferType == addressTransferType.Value);
			}

			if(categories != null)
			{
				query.Inner.JoinAlias(() => driverTransferDocItemAlias.Nomenclature, () => nomenclatureAlias);
				query.WhereRestrictionOn(() => nomenclatureAlias.Category).IsIn(categories);
			}

			var result = query
				.SelectList(list => list
					.SelectGroup(() => driverTransferDocItemAlias.Nomenclature.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
					.SelectSum(() => driverTransferDocItemAlias.Amount).WithAlias(() => resultNodeAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();

			return result;
		}

		public IEnumerable<GoodsInRouteListResult> AllGoodsTransferredFromDrivers(IUnitOfWork uow, RouteList routeList,
			NomenclatureCategory[] categories = null, AddressTransferType? addressTransferType = null)
		{
			AddressTransferDocument transferDocAlias = null;
			AddressTransferDocumentItem transferDocItemAlias = null;
			DriverNomenclatureTransferItem driverTransferDocItemAlias = null;
			GoodsInRouteListResult resultNodeAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => transferDocAlias)
				.JoinAlias(() => transferDocAlias.AddressTransferDocumentItems, () => transferDocItemAlias)
				.JoinAlias(() => transferDocItemAlias.DriverNomenclatureTransferDocumentItems, () => driverTransferDocItemAlias)
				.Where(() => transferDocAlias.RouteListTo.Id == routeList.Id);

			if(addressTransferType.HasValue)
			{
				query.Where(() => transferDocItemAlias.AddressTransferType == addressTransferType.Value);
			}

			if(categories != null)
			{
				query.Inner.JoinAlias(() => driverTransferDocItemAlias.Nomenclature, () => nomenclatureAlias);
				query.WhereRestrictionOn(() => nomenclatureAlias.Category).IsIn(categories);
			}

			var result = query
				.SelectList(list => list
					.SelectGroup(() => driverTransferDocItemAlias.Nomenclature.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
					.SelectSum(() => driverTransferDocItemAlias.Amount).WithAlias(() => resultNodeAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();

			return result;
		}

		public List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, NomenclatureCategory[] categories = null, int[] excludeNomenclatureIds = null)
		{
			Nomenclature nomenclatureAlias = null;
			ReturnsNode resultAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseBulkGoodsAccountingOperation movementOperationAlias = null;

			var returnableQuery = uow.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.GoodsAccountingOperation, () => movementOperationAlias)
				.Where(() => movementOperationAlias.Amount > 0)
				.JoinAlias(() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => !nomenclatureAlias.IsSerial);

			if(categories != null)
			{
				returnableQuery.Where(() => nomenclatureAlias.Category.IsIn(categories));
			}

			if(excludeNomenclatureIds != null)
			{
				returnableQuery.Where(() => !nomenclatureAlias.Id.IsIn(excludeNomenclatureIds));
			}

			var result = returnableQuery.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => false).WithAlias(() => resultAlias.Trackable)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
				.List<ReturnsNode>();

			DomainHelper.FillPropertyByEntity<ReturnsNode, Nomenclature>(uow, result, x => x.NomenclatureId, (node, nom) => node.Nomenclature = nom);
			return result.ToList();
		}

		/// <summary>
		/// Возвращает список товаров возвращенного на склад по номенклатурам
		/// </summary>
		public List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, params int[] nomenclatureIds)
		{
			List<ReturnsNode> result = new List<ReturnsNode>();
			Nomenclature nomenclatureAlias = null;
			ReturnsNode resultAlias = null;
			CarUnloadDocument carUnloadAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseBulkGoodsAccountingOperation movementOperationAlias = null;

			var returnableQuery = QueryOver.Of(() => carUnloadAlias)
				.JoinAlias(() => carUnloadAlias.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.GoodsAccountingOperation, () => movementOperationAlias)
				.JoinAlias(() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => !nomenclatureAlias.IsSerial)
				.Where(() => carUnloadAlias.RouteList.Id == routeListId)
				.Where(() => nomenclatureAlias.Id.IsIn(nomenclatureIds))
				.GetExecutableQueryOver(uow.Session);

			var returnableItems = returnableQuery.SelectList
			(
				list => list.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => false).WithAlias(() => resultAlias.Trackable)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.SelectGroup(() => carUnloadItemsAlias.DefectSource).WithAlias(() => resultAlias.DefectSource)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
			)
			.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
			.List<ReturnsNode>();

			result.AddRange(returnableItems);
			DomainHelper.FillPropertyByEntity<ReturnsNode, Nomenclature>(uow, result, x => x.NomenclatureId, (node, nom) => node.Nomenclature = nom);
			return result;
		}

		public int BottlesUnloadedByCarUnloadedDocuments(IUnitOfWork uow, int emptyBottleId, int routeListId, params int[] exceptDocumentIds)
		{

			GoodsAccountingOperation movementOperationAlias = null;

			var returnableQuery = OrderItemsReturnedToAllWarehouses(uow, routeListId, emptyBottleId).GetExecutableQueryOver(uow.Session);
			var result = returnableQuery.WhereNot(x => x.Id.IsIn(exceptDocumentIds))
										.SelectList(list => list.SelectSum(() => movementOperationAlias.Amount))
										.SingleOrDefault<decimal>()
										;
			return (int)result;
		}

		public QueryOver<CarUnloadDocument> OrderItemsReturnedToAllWarehouses(IUnitOfWork uow, int routeListId, params int[] nomenclatureIds)
		{
			Nomenclature nomenclatureAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseBulkGoodsAccountingOperation movementOperationAlias = null;

			var returnableQuery = QueryOver.Of<CarUnloadDocument>()
			   .Where(doc => doc.RouteList.Id == routeListId)
			   .JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
			   .JoinAlias(() => carUnloadItemsAlias.GoodsAccountingOperation, () => movementOperationAlias)
			   .Where(() => movementOperationAlias.Amount > 0)
			   .JoinAlias(() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
			   .Where(() => !nomenclatureAlias.IsSerial)
			   .Where(() => nomenclatureAlias.Id.IsIn(nomenclatureIds));

			return returnableQuery;
		}

		public IEnumerable<CarLoadDocument> GetCarLoadDocuments(IUnitOfWork uow, int routelistId)
		{
			return uow.Session.QueryOver<CarLoadDocument>()
			   .Where(x => x.RouteList.Id == routelistId)
			   .List();
		}

		public RouteList GetActualRouteListByOrder(IUnitOfWork uow, VodovozOrder order)
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			return uow.Session.QueryOver(() => routeListAlias)
							  .Left.JoinAlias(() => routeListAlias.Addresses, () => routeListItemAlias)
							  .Where(() => routeListItemAlias.Order.Id == order.Id)
							  .And(() => routeListItemAlias.Status != RouteListItemStatus.Transfered)
							  .Fetch(SelectMode.ChildFetch, routeList => routeList.Addresses)
							  .List()
							  .FirstOrDefault();
		}

		public RouteList GetActualRouteListByOrder(IUnitOfWork uow, int orderId)
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			return uow.Session.QueryOver(() => routeListAlias)
							  .Left.JoinAlias(() => routeListAlias.Addresses, () => routeListItemAlias)
							  .Where(() => routeListItemAlias.Order.Id == orderId)
							  .And(() => routeListItemAlias.Status != RouteListItemStatus.Transfered)
							  .Fetch(SelectMode.ChildFetch, routeList => routeList.Addresses)
							  .List()
							  .FirstOrDefault();
		}

		public bool RouteListWasChanged(RouteList routeList)
		{
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				var actualRouteList = uow.GetById<RouteList>(routeList.Id);
				return actualRouteList.Version != routeList.Version;
			}
		}

		public IList<RouteList> GetRouteListsByIds(IUnitOfWork uow, int[] routeListsIds)
		{
			RouteList routeListAlias = null;
			var query = uow.Session.QueryOver(() => routeListAlias)
				.Where(
					Restrictions.In(
						Projections.Property(() => routeListAlias.Id),
						routeListsIds
						)
					);

			return query.List();
		}

		public RouteList GetRouteListById(IUnitOfWork uow, int routeListsId)
		{
			return uow.GetById<RouteList>(routeListsId);
		}

		public IEnumerable<KeyValuePair<string, int>> GetDeliveryItemsToReturn(IUnitOfWork unitOfWork, int routeListsId)
		{
			RouteListItem routeListItemAlias = null;
			VodovozOrder orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			KeyValuePair<string, int> keyValuePairAlias = new KeyValuePair<string, int>();

			return unitOfWork.Session.QueryOver(() => routeListItemAlias)
				.Inner.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Inner.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.Inner.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(Restrictions.Or(
						Restrictions.And(
							Restrictions.Eq(Projections.Property(() => orderAlias.OrderStatus), OrderStatus.Shipped),
							Restrictions.Eq(Projections.Property(() => orderEquipmentAlias.Direction), Direction.PickUp)
						),
						Restrictions.And(
							Restrictions.Eq(Projections.Property(() => orderAlias.OrderStatus), OrderStatus.NotDelivered),
							Restrictions.Eq(Projections.Property(() => orderEquipmentAlias.Direction), Direction.Deliver)
						)
					))
				.And(Restrictions.Eq(Projections.Property(() => routeListItemAlias.RouteList.Id), routeListsId))
				.SelectList(list =>
					list.Select(Projections.GroupProperty(Projections.Property(() => nomenclatureAlias.Name)).WithAlias(() => keyValuePairAlias.Key))
						.Select(Projections.Sum(
							Projections.Conditional(
								Restrictions.Eq(Projections.Property(() => orderEquipmentAlias.Direction), Direction.PickUp),
								Projections.Property(() => orderEquipmentAlias.ActualCount),
								Projections.Conditional(
									Restrictions.Eq(Projections.Property(() => orderEquipmentAlias.Direction), Direction.Deliver),
									Projections.Property(() => orderEquipmentAlias.Count),
									Projections.Constant(0))
								)
							)
						).WithAlias(() => keyValuePairAlias.Value)
				).TransformUsing(Transformers.AliasToBean<KeyValuePair<string, int>>()).List<KeyValuePair<string, int>>();
		}

		public bool RouteListContainsGivenFuelLiters(IUnitOfWork uow, int routeListId)
		{
			var result = false;

			var fuelOperations = uow.Session.QueryOver<FuelDocument>()
				.Where(x => x.RouteList.Id == routeListId)
				.Select(f => f.FuelOperation)
				.List<FuelOperation>();

			foreach(var operation in fuelOperations)
			{
				var litersGiven = operation.LitersGived;
				var payedLiters = operation.PayedLiters;

				if(litersGiven > 0 || payedLiters > 0)
				{
					result = true;
				}
			}

			return result;
		}

		public SelfDriverTerminalTransferDocument GetSelfDriverTerminalTransferDocument(IUnitOfWork unitOfWork, Employee driver, RouteList routeList) =>
			unitOfWork.Session.QueryOver<SelfDriverTerminalTransferDocument>()
				.Where(d => d.DriverTo == driver && d.RouteListTo == routeList)
				.SingleOrDefault();

		public IList<NewDriverAdvanceRouteListNode> GetOldUnclosedRouteLists(IUnitOfWork uow, DateTime routeListDate, int driverId)
		{
			NewDriverAdvanceRouteListNode resultAlias = null;

			var unclosedRouteLists = uow.Session.QueryOver<RouteList>()
				.Where(x => x.Date < routeListDate)
				.And(x => x.Status != RouteListStatus.Closed)
				.And(x => x.Driver.Id == driverId)
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Date).WithAlias(() => resultAlias.Date)
				)
				.TransformUsing(Transformers.AliasToBean<NewDriverAdvanceRouteListNode>())
				.List<NewDriverAdvanceRouteListNode>();

			return unclosedRouteLists;
		}

		public bool HasEmployeeAdvance(IUnitOfWork uow, int routeListId, int driverId) =>
			uow.Session.QueryOver<Expense>()
				.Where(x => x.RouteListClosing.Id == routeListId)
				.And(x => x.Employee.Id == driverId)
				.And(x => x.TypeOperation == ExpenseType.EmployeeAdvance)
				.RowCount() > 0;

		public DateTime? GetDateByDriverWorkingDayNumber(IUnitOfWork uow, int driverId, int dayNumber, CarTypeOfUse? driverOfCarTypeOfUse = null,
			CarOwnType? driverOfCarOwnType = null)
		{
			Employee employeeAlias = null;

			var query = uow.Session.QueryOver<RouteList>()
				.JoinAlias(x => x.Driver, () => employeeAlias)
				.Where(x => x.Driver.Id == driverId);

			if(driverOfCarTypeOfUse != null)
			{
				query.Where(() => employeeAlias.DriverOfCarTypeOfUse == driverOfCarTypeOfUse.Value);
			}

			if(driverOfCarOwnType != null)
			{
				query.Where(() => employeeAlias.DriverOfCarOwnType == driverOfCarOwnType.Value);
			}

			return query
				.SelectList(list => list
					.SelectGroup(x => x.Date))
				.OrderBy(x => x.Date).Asc
				.Skip(dayNumber - 1)
				.Take(1)
				.SingleOrDefault<DateTime?>();
		}

		public DateTime? GetLastRouteListDateByDriver(IUnitOfWork uow, int driverId, CarTypeOfUse? driverOfCarTypeOfUse = null,
			CarOwnType? driverOfCarOwnType = null)
		{
			Employee employeeAlias = null;

			var query = uow.Session.QueryOver<RouteList>()
				.JoinAlias(x => x.Driver, () => employeeAlias)
				.Where(x => x.Driver.Id == driverId);

			if(driverOfCarTypeOfUse != null)
			{
				query.Where(() => employeeAlias.DriverOfCarTypeOfUse == driverOfCarTypeOfUse.Value);
			}

			if(driverOfCarOwnType != null)
			{
				query.Where(() => employeeAlias.DriverOfCarOwnType == driverOfCarOwnType.Value);
			}

			return query
				.SelectList(list => list
					.Select(x => x.Date))
				.OrderBy(x => x.Date).Desc
				.Take(1)
				.SingleOrDefault<DateTime?>();
		}

		public IList<RouteList> GetRouteListsForCarInPeriods(IUnitOfWork uow, int carId,
			IList<(DateTime startDate, DateTime? endDate)> periods)
		{
			RouteList routeListAlias = null;

			var query = uow.Session.QueryOver(() => routeListAlias)
				.Where(x => x.Car.Id == carId);

			Disjunction periodsDisjunction = new Disjunction();
			foreach(var (startDate, endDate) in periods)
			{
				var conjunction = new Conjunction();
				conjunction.Add(() => routeListAlias.Date >= startDate);
				if(endDate != null)
				{
					conjunction.Add(() => routeListAlias.Date <= endDate.Value);
				}
				periodsDisjunction.Add(conjunction);
			}

			if(periods.Any())
			{
				query.Where(periodsDisjunction);
			}

			return query.List<RouteList>();
		}

		public IList<Employee> GetDriversWithAdditionalLoading(IUnitOfWork uow, params int[] routeListIds)
		{
			RouteList routeListAlias = null;

			var query = uow.Session.QueryOver(() => routeListAlias);

			if(routeListIds.Length > 0)
			{
				query.WhereRestrictionOn(() => routeListAlias.Id).IsIn(routeListIds);
			}

			return query.Where(() => routeListAlias.AdditionalLoadingDocument != null)
				.Select(x => x.Driver)
				.List<Employee>();
		}

		public bool HasRouteList(int driverId, DateTime date, int deliveryShiftId)
		{
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				RouteList routeListAlias = null;

				var query = uow.Session.QueryOver(() => routeListAlias)
					.Where(() => routeListAlias.Date == date.Date)
					.Where(() => routeListAlias.Driver.Id == driverId)
					.Where(() => routeListAlias.Shift.Id == deliveryShiftId)
					.Select(Projections.Property(() => routeListAlias.Id));

				var result = query.List<int>();
				return result.Any();
			}
		}

		public decimal GetRouteListTotalSalesGoodsWeight(IUnitOfWork uow, int routeListId)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			RouteList rlAlias = null;

			var weightProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, ?2) * ?3"),
				NHibernateUtil.Decimal,
				Projections.Property(() => orderItemAlias.ActualCount),
				Projections.Property(() => orderItemAlias.Count),
				Projections.Property(() => nomenclatureAlias.Weight)
			);

			var weightOrderItems = QueryOver.Of<RouteListItem>()
				.Where(rla => rla.Status != RouteListItemStatus.Transfered)
				.And(rla => rla.RouteList.Id == rlAlias.Id)
				.JoinAlias(rla => rla.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Select(Projections.Sum(weightProjection));

			var total = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, ?2)"),
				NHibernateUtil.Decimal,
				Projections.SubQuery(weightOrderItems),
				Projections.Constant(0));

			var result = uow.Session.QueryOver(() => rlAlias)
				.Where(rl => rl.Id == routeListId)
				.Select(total)
				.SingleOrDefault<decimal>();

			return result;
		}

		public decimal GetRouteListPaidDeliveriesSum(IUnitOfWork uow, int routeListId, IEnumerable<int> paidDeliveriesNomenclaturesIds)
		{
			OrderItem orderItemAlias = null;
			VodovozOrder orderAlias = null;

			return uow.Session.QueryOver<RouteListItem>()
				.Where(rla => rla.RouteList.Id == routeListId)
				.And(rla => rla.Status != RouteListItemStatus.Transfered)
				.JoinAlias(rla => rla.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.WhereRestrictionOn(() => orderItemAlias.Nomenclature.Id).IsInG(paidDeliveriesNomenclaturesIds)
				.Select(OrderProjections.GetOrderSumProjection())
				.SingleOrDefault<decimal>();
		}

		public RouteListProfitabilitySpendings GetRouteListSpendings(IUnitOfWork uow, int routeListId, decimal routeListExpensesPerKg)
		{
			var sql = $@"
# При изменении этого запроса, необходимо внести соответствующие изменения в отчет по рентабельности
SELECT
	ROUND(SUM(prof.total_price), 2) as {nameof(RouteListProfitabilitySpendings.TotalSales)},
	ROUND(SUM(prof.total_purchase_price), 2) as {nameof(RouteListProfitabilitySpendings.TotalPurchaseSpending)},
	ROUND(SUM(prof.total_inner_delivery_price), 2) as {nameof(RouteListProfitabilitySpendings.TotalInnerDeliverySpending)},
	ROUND(SUM(prof.total_delivery_spending), 2) as {nameof(RouteListProfitabilitySpendings.TotalAddressDeliverySpending)},
	ROUND(SUM(prof.total_warehouse_spending), 2) as {nameof(RouteListProfitabilitySpendings.TotalWarehouseSpending)},
	ROUND(SUM(prof.total_administrative_spending), 2) as {nameof(RouteListProfitabilitySpendings.TotalAdministrativeSpending)}
FROM
	(SELECT
		IFNULL(oi.actual_count, oi.count) AS items_count,
		(SELECT ((items_count * oi.price) - oi.discount_money)) AS total_price, 
		#Пр-во/закупка - если групповая себестоимость то себестоимость, иначе цена закупки
		(
			IF(n.using_in_group_price_set,
				(
					SELECT ncp.cost_price
					FROM nomenclature_cost_prices ncp 
					WHERE ncp.nomenclature_id = n.id 
					AND ncp.start_date <= o.delivery_date 
					AND (ncp.end_date IS NULL OR ncp.end_date > o.delivery_date)
				),
				(
					SELECT npp.purchase_price
					FROM nomenclature_purchase_prices npp 
					WHERE npp.nomenclature_id = n.id 
					AND npp.start_date <= o.delivery_date 
					AND (npp.end_date IS NULL OR npp.end_date > o.delivery_date)
				)
			)
		) AS cost_purchase_price,
		(SELECT CAST((cost_purchase_price * items_count) AS DECIMAL(12,2))) AS total_purchase_price,
		#Фура - стоимость доставки до склада
		(
			SELECT 
				IFNULL(nidp.price, '-')
			FROM nomenclature_inner_delivery_prices nidp  
			WHERE nidp.nomenclature_id = n.id 
			AND nidp.start_date <= o.delivery_date 
			AND (nidp.end_date IS NULL OR nidp.end_date > o.delivery_date)
		) AS inner_delivery_price_source,
		(SELECT IF(w.type_of_use = 'Production', 0, inner_delivery_price_source)) AS inner_delivery_price,
		(SELECT CAST((inner_delivery_price * items_count) AS DECIMAL(12,2))) AS total_inner_delivery_price,
		#затраты на адресную доставку на килограмм
		(SELECT IF(o.self_delivery, 0, :route_list_expenses_per_kg)) AS route_list_expenses_per_kg,
		(SELECT CAST(weight * route_list_expenses_per_kg AS DECIMAL(12,2))) AS delivery_spending,
		(SELECT CAST((delivery_spending * items_count) AS DECIMAL(12,2))) AS total_delivery_spending,
		#складские расходы на килограмм
		(
			SELECT		
				pc.warehouse_expenses_per_kg
			FROM 
				profitability_constants pc
			WHERE
				YEAR(pc.calculated_month) = YEAR(o.delivery_date) 
				AND MONTH(pc.calculated_month) = MONTH(o.delivery_date)
		) AS warehouse_expenses_per_kg_source,	
		(
			SELECT 
				IFNULL(warehouse_expenses_per_kg_source, 
				(
					SELECT pc.warehouse_expenses_per_kg
					FROM profitability_constants pc
					ORDER BY pc.calculated_month DESC
					LIMIT 1
				)
			)
		) AS warehouse_expenses_per_kg_last_calc,
		(SELECT IF(w.type_of_use = 'Production', 0, warehouse_expenses_per_kg_last_calc)) AS warehouse_expenses_per_kg,
		(SELECT CAST(weight * warehouse_expenses_per_kg AS DECIMAL(12,2))) AS warehouse_spending,
		(SELECT CAST((warehouse_spending * items_count) AS DECIMAL(12,2))) AS total_warehouse_spending,
		#административные расходы на килограмм
		(
			SELECT		
				pc.administrative_expenses_per_kg
			FROM 
				profitability_constants pc
			WHERE
				YEAR(pc.calculated_month) = YEAR(o.delivery_date) 
				AND MONTH(pc.calculated_month) = MONTH(o.delivery_date)
		) AS administrative_expenses_per_kg_source,	
		(
			SELECT 
				IFNULL(administrative_expenses_per_kg_source, 
				(
					SELECT pc.administrative_expenses_per_kg
					FROM profitability_constants pc
					ORDER BY pc.calculated_month DESC
					LIMIT 1
				)
			)
		) AS administrative_expenses_per_kg,
		(SELECT CAST(weight * administrative_expenses_per_kg AS DECIMAL(12,2))) AS administrative_spending,
		(SELECT CAST((administrative_spending * items_count) AS DECIMAL(12,2))) AS total_administrative_spending
	FROM
		order_items oi
		LEFT JOIN
		orders o ON order_id = o.id
		LEFT JOIN
		route_list_addresses rla ON rla.order_id = o.id AND rla.status  not in ('Transfered', 'Canceled', 'Overdue')
		LEFT JOIN
		nomenclature n ON oi.nomenclature_id = n.id
		LEFT JOIN
		store_car_load_documents scld ON scld.route_list_id = rla.route_list_id
		LEFT JOIN
		warehouses w ON w.id = scld.warehouse_id
	WHERE rla.route_list_id = :route_list_id
		AND n.category != 'deposit'
	GROUP BY oi.id
	) prof
;";

			var query = uow.Session.CreateSQLQuery(sql)
				.SetParameter("route_list_id", routeListId)
				.SetParameter("route_list_expenses_per_kg", routeListExpensesPerKg)
				.SetResultTransformer(Transformers.AliasToBean(typeof(RouteListProfitabilitySpendings)));

			var result = query.UniqueResult<RouteListProfitabilitySpendings>();
			return result;
		}

		public decimal GetRouteListSalesSum(IUnitOfWork uow, int routeListId)
		{
			OrderItem orderItemAlias = null;
			VodovozOrder orderAlias = null;

			return uow.Session.QueryOver<RouteListItem>()
				.Where(rla => rla.RouteList.Id == routeListId)
				.AndRestrictionOn(rla => rla.Status).Not.IsInG(RouteListItem.GetNotDeliveredStatuses())
				.JoinAlias(rla => rla.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Select(OrderProjections.GetOrderSumProjection())
				.SingleOrDefault<decimal>();
		}

		public AddressTransferType[] AddressTransferTypesWithoutTransferFromHandToHand =>
			new[]
			{
				AddressTransferType.NeedToReload,
				AddressTransferType.FromFreeBalance
			};

		public int GetUnclosedRouteListsCountHavingDebtByDriver(IUnitOfWork uow, int driverId, int excludeRouteListId = 0)
		{
			RouteList routeListAlias = null;
			RouteListDebt routeListDebtAlias = null;

			var routeListsCount = uow.Session.QueryOver(() => routeListDebtAlias)
				.JoinAlias(() => routeListDebtAlias.RouteList, () => routeListAlias)
				.Where(() =>
					routeListAlias.Driver.Id == driverId
					&& routeListAlias.Status != RouteListStatus.Closed
					&& routeListAlias.Id != excludeRouteListId
					&& routeListDebtAlias.Debt > 0)
				.Select(Projections.Count(() => routeListDebtAlias.RouteList))
				.SingleOrDefault<int>();

			return routeListsCount;
		}

		public decimal GetUnclosedRouteListsDebtsSumByDriver(IUnitOfWork uow, int driverId, int excludeRouteListId = 0)
		{
			RouteList routeListAlias = null;
			RouteListDebt routeListDebtAlias = null;

			var routeListsDebtsSum = uow.Session.QueryOver(() => routeListDebtAlias)
				.JoinAlias(() => routeListDebtAlias.RouteList, () => routeListAlias)
				.Where(() =>
					routeListAlias.Driver.Id == driverId
					&& routeListAlias.Status != RouteListStatus.Closed
					&& routeListAlias.Id != excludeRouteListId
					&& routeListDebtAlias.Debt > 0)
				.Select(Projections.Sum(() => routeListDebtAlias.Debt))
				.SingleOrDefault<decimal>();

			return routeListsDebtsSum;
		}

		public IList<Nomenclature> GetRouteListNomenclatures(IUnitOfWork uow, int routeListId, bool isArchived = false)
		{
			RouteListItem routeListItemAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => nomenclatureAlias)
				.JoinEntityAlias(() => orderItemAlias, () => orderItemAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.JoinEntityAlias(() => routeListItemAlias, () => routeListItemAlias.Order.Id == orderItemAlias.Order.Id)
				.Where(() => routeListItemAlias.RouteList.Id == routeListId);

			if(isArchived)
			{
				query.Where(() => nomenclatureAlias.IsArchive);
			}

			var result = query.TransformUsing(Transformers.DistinctRootEntity).List();

			return result;
		}

		private string GetCargoDailyNormParameterName(CarTypeOfUse carTypeOfUse) => $"CargoDailyNormFor{carTypeOfUse}";
		public decimal GetCargoDailyNorm(CarTypeOfUse carTypeOfUse) => _settingsController.GetDecimalValue(GetCargoDailyNormParameterName(carTypeOfUse));

		public void SaveCargoDailyNorms(Dictionary<CarTypeOfUse, decimal> cargoDailyNorms)
		{
			foreach(var cargoDailyNorm in cargoDailyNorms)
			{
				_settingsController.CreateOrUpdateSetting(GetCargoDailyNormParameterName(cargoDailyNorm.Key), cargoDailyNorm.Value.ToString());
			}
		}

		public async Task<IList<RouteList>> GetCarsRouteListsForPeriod(
			IUnitOfWork uow,
			CarTypeOfUse[] carTypesOfUse,
			CarOwnType[] carOwnTypes,
			Car car,
			int[] includedCarModelIds,
			int[] excludedCarModelIds,
			DateTime startDate,
			DateTime endDate,
			bool isOnlyCarsWithCompletedFastDelivery,
			bool isOnlyCarsWithCompletedCommonDelivery,
			CancellationToken cancellationToken)
		{
			return await Task.Run(() =>
			{
				RouteList routeListAlias = null;
				RouteListItem routeListAddressAlias = null;
				VodovozOrder orderAlias = null;
				Car carAlias = null;
				CarModel carModelAlias = null;
				CarVersion carVersionAlias = null;
				Employee assignedDriverAlias = null;

				var query = uow.Session.QueryOver(() => routeListAlias)
					.Inner.JoinAlias(() => routeListAlias.Car, () => carAlias)
					.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
					.Left.JoinAlias(() => carAlias.Driver, () => assignedDriverAlias)
					.JoinEntityAlias(() => carVersionAlias,
						() => carVersionAlias.Car.Id == carAlias.Id
							&& carVersionAlias.StartDate <= routeListAlias.Date &&
							(carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date))
					.Where(() => routeListAlias.Date >= startDate && routeListAlias.Date < endDate)
					.Where(() => !carAlias.IsArchive)
					.And(() => carModelAlias.CarTypeOfUse != CarTypeOfUse.Truck)
					.And(() => assignedDriverAlias.Id == null || !assignedDriverAlias.VisitingMaster)
					.And(Restrictions.In(Projections.Property(() => carVersionAlias.CarOwnType), carOwnTypes))
					.And(Restrictions.In(Projections.Property(() => carModelAlias.CarTypeOfUse), carTypesOfUse));

				if(car != null)
				{
					query.Where(() => carAlias.Id == car.Id);
				}

				if(includedCarModelIds.Any())
				{
					query.Where(Restrictions.In(Projections.Property(() => carModelAlias.Id), includedCarModelIds));
				}

				if(excludedCarModelIds.Any())
				{
					query.Where(Restrictions.Not(Restrictions.In(Projections.Property(() => carModelAlias.Id), excludedCarModelIds)));
				}

				var completedFastDeliveryAddressesSubquery =
					QueryOver.Of(() => routeListAddressAlias)
					.Left.JoinAlias(() => routeListAddressAlias.Order, () => orderAlias)
					.Where(() => routeListAddressAlias.RouteList.Id == routeListAlias.Id)
					.And(() => routeListAddressAlias.Status == RouteListItemStatus.Completed)
					.And(() => orderAlias.IsFastDelivery)
					.Select(rla => rla.Id)
					.Take(1);

				var completedCommonAddressesSubquery =
					QueryOver.Of(() => routeListAddressAlias)
					.Left.JoinAlias(() => routeListAddressAlias.Order, () => orderAlias)
					.Where(() => routeListAddressAlias.RouteList.Id == routeListAlias.Id)
					.And(() => routeListAddressAlias.Status == RouteListItemStatus.Completed)
					.And(() => !orderAlias.IsFastDelivery)
					.Select(rla => rla.Id)
					.Take(1);

				if(isOnlyCarsWithCompletedFastDelivery && !isOnlyCarsWithCompletedCommonDelivery)
				{
					query.Where(Restrictions.IsNotNull(Projections.SubQuery(completedFastDeliveryAddressesSubquery)));
				}

				if(isOnlyCarsWithCompletedCommonDelivery && !isOnlyCarsWithCompletedFastDelivery)
				{
					query.Where(Restrictions.IsNotNull(Projections.SubQuery(completedCommonAddressesSubquery)));
				}

				return query
					.OrderBy(() => carAlias.Id).Asc
					.ThenBy(() => routeListAlias.Id).Asc
					.TransformUsing(Transformers.DistinctRootEntity)
					.List<RouteList>();
			},
				cancellationToken
			);
		}

		public IQueryable<ExploitationReportRouteListDataNode> GetExploitationReportRouteListDataNodes(
			IUnitOfWork unitOfWork,
			IEnumerable<int> routeListsIds)
		{
			var data =
				from rl in unitOfWork.Session.Query<RouteList>()
				join rla in unitOfWork.Session.Query<RouteListItem>() on rl.Id equals rla.RouteList.Id
				join car in unitOfWork.Session.Query<Car>() on rl.Car.Id equals car.Id
				join carModel in unitOfWork.Session.Query<CarModel>() on car.CarModel.Id equals carModel.Id
				join o in unitOfWork.Session.Query<VodovozOrder>() on rla.Order.Id equals o.Id into orders
				from order in orders.DefaultIfEmpty()
				join oi in unitOfWork.Session.Query<OrderItem>() on order.Id equals oi.Order.Id into orderItems
				from orderItem in orderItems.DefaultIfEmpty()
				join n in unitOfWork.Session.Query<Nomenclature>() on orderItem == null ? 0 : orderItem.Nomenclature.Id equals n.Id into nomenclatures
				from nomenclature in nomenclatures.DefaultIfEmpty()
				join deliveryPoint in unitOfWork.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals deliveryPoint.Id
				join d in unitOfWork.Session.Query<District>() on deliveryPoint.District.Id equals d.Id into districts
				from district in districts.DefaultIfEmpty()
				join ds in unitOfWork.Session.Query<DeliverySchedule>() on order.DeliverySchedule.Id equals ds.Id into deliverySchedules
				from deliverySchedule in deliverySchedules.DefaultIfEmpty()
				where routeListsIds.Contains(rl.Id)

				select new ExploitationReportRouteListDataNode
				{
					RouteListId = rla.RouteList.Id,
					RouteListDate = rl.Date,
					RouteListForwarderId = rl.Forwarder.Id,
					RouteListConfirmedDistance = rl.ConfirmedDistance,
					CarId = car.Id,
					CarTypeOfuse = carModel.CarTypeOfUse,
					CarMaxBottles = car.MaxBottles,
					CarMaxVolume = carModel.MaxVolume,
					CarMaxWeight = carModel.MaxWeight,
					AddressId = rla.Id,
					IsAddressWasTransfered = rla.WasTransfered,
					AddressStatus = rla.Status,
					AddressStatusLastUpdate = rla.StatusLastUpdate,
					OrderId = order.Id,
					OrderIsFastDelivery = order.IsFastDelivery,
					OrderItemCount = orderItem.Count,
					OrderItemActualCount = orderItem.ActualCount,
					IsNomenclatureWater19L = nomenclature == null ? false : nomenclature.IsWater19L,
					NomenclatureWeight = nomenclature == null ? 0 : nomenclature.Weight,
					NomenclatureVolume = nomenclature == null ? 0 : nomenclature.Volume,
					DeliveryPointId = deliveryPoint.Id,
					DeliveryPointDistrictId = deliveryPoint.District.Id,
					DeliveryPointWageDistrictId = district.WageDistrict.Id,
					OrderDeliveryScheduleTo = deliverySchedule.To
				};

			return data;
		}

		public IQueryable<int> GetOrderIdsByRouteLists(IUnitOfWork unitOfWork, IEnumerable<int> routeListsIds)
		{
			var ordersIds =
				from rla in unitOfWork.Session.Query<RouteListItem>()
				join order in unitOfWork.Session.Query<VodovozOrder>() on rla.Order.Id equals order.Id
				where routeListsIds.Contains(rla.RouteList.Id)
				orderby order.Id
				select order.Id;

			return ordersIds.Distinct();
		}

		public decimal GetCarsConfirmedDistanceForPeriod(IUnitOfWork unitOfWork, int carId, DateTime startDate, DateTime endDate)
		{
			return unitOfWork.Session.QueryOver<RouteList>()
				.Where(routeList => routeList.Car.Id == carId
					&& routeList.Date >= startDate
					&& routeList.Date < endDate
					&& routeList.ConfirmedDistance > 0)
				.Select(Projections.Sum<RouteList>(routeList => routeList.ConfirmedDistance))
				.SingleOrDefault<decimal>();
		}

		public IEnumerable<int> GetCompletedOrdersInTodayRouteListsByCarId(IUnitOfWork uow, int carId)
		{
			var query =
				from routeList in uow.Session.Query<RouteList>()
				join routeListAddress in uow.Session.Query<RouteListItem>() on routeList.Id equals routeListAddress.RouteList.Id
				where
					routeList.Date == DateTime.Today
					&& routeList.Car.Id == carId
					&& routeListAddress.Status == RouteListItemStatus.Completed
				select routeListAddress.Order.Id;

			return query.ToList();
		}
	}
}
