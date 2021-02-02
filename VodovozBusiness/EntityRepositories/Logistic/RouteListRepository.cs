using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Core.DataService;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Repositories;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class RouteListRepository : IRouteListRepository
	{
		public IList<RouteList> GetDriverRouteLists(IUnitOfWork uow, Employee driver, RouteListStatus status, DateTime date)
		{
			RouteList routeListAlias = null;

			return uow.Session.QueryOver<RouteList>(() => routeListAlias)
					  .Where(() => routeListAlias.Driver == driver)
					  .Where(() => routeListAlias.Status == status)
					  .Where(() => routeListAlias.Date == date)
					  .List();
		}

		public QueryOver<RouteList> GetRoutesAtDay(DateTime date)
		{
			return QueryOver.Of<RouteList>()
				.Where(x => x.Date == date);
		}

		public QueryOver<RouteList> GetRoutesAtDay(DateTime date, List<int> geographicGroupsIds)
		{
			GeographicGroup geographicGroupAlias = null;

			var query = QueryOver.Of<RouteList>()
								 .Where(x => x.Date == date)
								 ;

			if(geographicGroupsIds.Any()) {
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
			if (subdivisionRepository == null && warehouse != null)
			{
				throw new ArgumentNullException(nameof(subdivisionRepository));
			}

			List<GoodsInRouteListResultWithSpecialRequirements> result = new List<GoodsInRouteListResultWithSpecialRequirements>();

			GoodsInRouteListResultWithSpecialRequirements terminal = null;

			if (warehouse != null)
			{
				var cashSubdivisions = subdivisionRepository.GetCashSubdivisions(uow);
				if (cashSubdivisions.Contains(warehouse.OwningSubdivision))
				{
					terminal = GetTerminalInRLWithSpecialRequirements(uow, routeList, warehouse);
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

			if (terminal != null)
			{
				result.Add(terminal);
			}

			return result
				.GroupBy(
					x => x.NomenclatureName,
					x => x,
					(key, value) => new GoodsInRouteListResultWithSpecialRequirements()
					{
						NomenclatureName = key,
						NomenclatureId = value.FirstOrDefault().NomenclatureId,
						ExpireDatePercent = value.FirstOrDefault().ExpireDatePercent,
						Amount = value.Sum(x => x.Amount)
					}).ToList();
		}

		public IList<GoodsInRouteListResult> GetGoodsAndEquipsInRL(
			IUnitOfWork uow,
			RouteList routeList,
			ISubdivisionRepository subdivisionRepository = null,
			Warehouse warehouse = null)
		{
			if(subdivisionRepository == null && warehouse != null) {
				throw new ArgumentNullException(nameof(subdivisionRepository));
			}

			List<GoodsInRouteListResult> result = new List<GoodsInRouteListResult>();

			if(warehouse != null) {
				var cashSubdivisions = subdivisionRepository.GetCashSubdivisions(uow);
				if(cashSubdivisions.Contains(warehouse.OwningSubdivision)) {
					var terminal = GetTerminalInRL(uow, routeList, warehouse);
					if(terminal != null)
						result.Add(terminal);
				}
				else {
					result.AddRange(GetGoodsInRLWithoutEquipments(uow, routeList).ToList());
					result.AddRange(GetEquipmentsInRL(uow, routeList).ToList());
				}
			}
			else {
				result.AddRange(GetGoodsInRLWithoutEquipments(uow, routeList).ToList());
				result.AddRange(GetEquipmentsInRL(uow, routeList).ToList());

				var terminal = GetTerminalInRL(uow, routeList);
				if(terminal != null)
					result.Add(terminal);
			}

			return result
				.GroupBy(x => x.NomenclatureId, x => x.Amount)
				.Select(x => new GoodsInRouteListResult {
						NomenclatureId = x.Key,
						Amount = x.Sum()
					}
				)
				.ToList();
		}

		public IList<GoodsInRouteListResult> GetGoodsInRLWithoutEquipments(IUnitOfWork uow, RouteList routeList)
		{
			GoodsInRouteListResult resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of(() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<RouteListItem>()
				.Where(r => r.RouteList.Id == routeList.Id)
				.Where(r => !r.WasTransfered || (r.WasTransfered && r.NeedToReload))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderitemsQuery = uow.Session.QueryOver<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Where(() => OrderItemNomenclatureAlias.Category.IsIn(Nomenclature.GetCategoriesForShipment()));

			return orderitemsQuery.SelectList(list => list
				.SelectGroup(() => OrderItemNomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.SelectSum(() => orderItemsAlias.Count).WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();
		}

		public IList<GoodsInRouteListResultWithSpecialRequirements> GetGoodsInRLWithoutEquipmentsWithSpecialRequirements(IUnitOfWork uow, RouteList routeList)
		{
			GoodsInRouteListResultWithSpecialRequirements resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			Counterparty counterpartyAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of(() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<RouteListItem>()
				.Where(r => r.RouteList.Id == routeList.Id)
				.Where(r => !r.WasTransfered || (r.WasTransfered && r.NeedToReload))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderitemsQuery = uow.Session.QueryOver<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.JoinAlias(() => orderItemsAlias.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Where(() => OrderItemNomenclatureAlias.Category.IsIn(Nomenclature.GetCategoriesForShipment()));

			return orderitemsQuery.SelectList(list => list
				.Select(
					Projections.GroupProperty(
						Projections.Conditional(
							Restrictions.And(
								Restrictions.Where(() => counterpartyAlias.SpecialExpireDatePercentCheck),
								Restrictions.Where(() => OrderItemNomenclatureAlias.Category == NomenclatureCategory.water)
							),
							Projections.SqlFunction( 
								new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT(?1, ' >', ?2, '% срока годности')"),
								NHibernateUtil.String,
								Projections.Property(() => OrderItemNomenclatureAlias.Name),
								Projections.Cast(NHibernateUtil.String, Projections.Property(() => counterpartyAlias.SpecialExpireDatePercent))
							), 
							Projections.Property(() => OrderItemNomenclatureAlias.Name)
						)
					)
				).WithAlias(() => resultAlias.NomenclatureName)
				.Select(
					Projections.Conditional(
						Restrictions.And(
							Restrictions.Where(() => counterpartyAlias.SpecialExpireDatePercentCheck),
							Restrictions.Where(() => OrderItemNomenclatureAlias.Category == NomenclatureCategory.water)
						),
						Projections.Property(() => counterpartyAlias.SpecialExpireDatePercent),
						Projections.Constant(null, NHibernateUtil.Decimal)
					)
				).WithAlias(() => resultAlias.ExpireDatePercent)
				.Select(() => OrderItemNomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.SelectSum(() => orderItemsAlias.Count).WithAlias(() => resultAlias.Amount))
			    .TransformUsing(Transformers.AliasToBean<GoodsInRouteListResultWithSpecialRequirements>())
				.List<GoodsInRouteListResultWithSpecialRequirements>();
		}

		public IList<GoodsInRouteListResultWithSpecialRequirements> GetEquipmentsInRLWithSpecialRequirements(IUnitOfWork uow, RouteList routeList)
		{
			GoodsInRouteListResultWithSpecialRequirements resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderEquipmentNomenclatureAlias = null;

			//Выбирается список Id заказов находящихся в МЛ
			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => orderAlias);
			var routeListItemsSubQuery = QueryOver.Of<RouteListItem>()
				.Where(r => r.RouteList.Id == routeList.Id)
				.Where(r => !r.WasTransfered || (r.WasTransfered && r.NeedToReload))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderEquipmentsQuery = uow.Session.QueryOver<OrderEquipment>(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => OrderEquipmentNomenclatureAlias);

			return orderEquipmentsQuery
				.SelectList(list => list
				   .SelectGroup(() => OrderEquipmentNomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
							.Select(Projections.Sum(
								Projections.Cast(NHibernateUtil.Decimal, Projections.Property(() => orderEquipmentAlias.Count))
							)).WithAlias(() => resultAlias.Amount)
							.Select(() => OrderEquipmentNomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)

				)
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResultWithSpecialRequirements>())
				.List<GoodsInRouteListResultWithSpecialRequirements>();
		}

		public IList<GoodsInRouteListResult> GetEquipmentsInRL(IUnitOfWork uow, RouteList routeList)
		{
			GoodsInRouteListResult resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderEquipmentNomenclatureAlias = null;

			//Выбирается список Id заказов находящихся в МЛ
			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => orderAlias);
			var routeListItemsSubQuery = QueryOver.Of<RouteListItem>()
				.Where(r => r.RouteList.Id == routeList.Id)
				.Where(r => !r.WasTransfered || (r.WasTransfered && r.NeedToReload))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderEquipmentsQuery = uow.Session.QueryOver<OrderEquipment>(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => OrderEquipmentNomenclatureAlias);
				
			return orderEquipmentsQuery
				.SelectList(list => list
				   .SelectGroup(() => OrderEquipmentNomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
							.Select(Projections.Sum(
					   Projections.Cast(NHibernateUtil.Decimal, Projections.Property(() => orderEquipmentAlias.Count)))).WithAlias(() => resultAlias.Amount)
				)
				.TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();
		}
		
		public GoodsInRouteListResult GetTerminalInRL(IUnitOfWork uow, RouteList routeList, Warehouse warehouse = null) {
			CarLoadDocumentItem carLoadDocumentItemAlias = null;
			
			var terminalId = new BaseParametersProvider().GetNomenclatureIdForTerminal;
			var needTerminal = routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

			var loadedTerminal = uow.Session.QueryOver<CarLoadDocument>()
			                        .JoinAlias(x => x.Items, () => carLoadDocumentItemAlias)
			                        .Where(() => carLoadDocumentItemAlias.Nomenclature.Id == terminalId)
			                        .And(x => x.RouteList.Id == routeList.Id)
			                        .List();
			
			if (needTerminal || loadedTerminal.Any()) {
				var terminal = uow.GetById<Nomenclature>(terminalId);
				int amount = 1;

				if(warehouse == null) {
					return new GoodsInRouteListResult {
						NomenclatureId = terminalId,
						Amount = amount
					};
				}


				if(StockRepository.NomenclatureInStock(uow, warehouse.Id, new int[] { terminal.Id }).Any()) {
					return new GoodsInRouteListResult {
						NomenclatureId = terminalId,
						Amount = amount
					};
				}
			}

			return null;
		}

        public bool IsTerminalRequired(IUnitOfWork uow, RouteList routeList)
        {
            throw new NotImplementedException();
        }

		public GoodsInRouteListResultWithSpecialRequirements GetTerminalInRLWithSpecialRequirements(IUnitOfWork uow, RouteList routeList, Warehouse warehouse = null)
		{
			CarLoadDocumentItem carLoadDocumentItemAlias = null;

			var terminalId = new BaseParametersProvider().GetNomenclatureIdForTerminal;
			var needTerminal = routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

			var loadedTerminal = uow.Session.QueryOver<CarLoadDocument>()
									.JoinAlias(x => x.Items, () => carLoadDocumentItemAlias)
									.Where(() => carLoadDocumentItemAlias.Nomenclature.Id == terminalId)
									.And(x => x.RouteList.Id == routeList.Id)
									.List();

			if (needTerminal || loadedTerminal.Any())
			{
				var terminal = uow.GetById<Nomenclature>(terminalId);
				int amount = 1;

				if (warehouse == null)
				{
					return new GoodsInRouteListResultWithSpecialRequirements
					{
						NomenclatureName = terminal.Name,
						NomenclatureId = terminalId,
						Amount = amount
					};
				}


				if (StockRepository.NomenclatureInStock(uow, warehouse.Id, new int[] { terminal.Id }).Any())
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



		public IList<GoodsInRouteListResult> AllGoodsLoaded(IUnitOfWork uow, RouteList routeList, CarLoadDocument excludeDoc = null)
		{
			CarLoadDocument docAlias = null;
			CarLoadDocumentItem docItemsAlias = null;
			GoodsInRouteListResult inCarLoads = null;

			var loadedQuery = uow.Session.QueryOver<CarLoadDocument>(() => docAlias)
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
		
		public IEnumerable<GoodsInRouteListResult> AllGoodsDelivered(IUnitOfWork uow, RouteList routeList)
		{
			if(routeList == null) throw new ArgumentNullException(nameof(routeList));
			
			IList<GoodsInRouteListResult> result = new List<GoodsInRouteListResult>();
			if(!routeList.Addresses.Any()) {
				return result;
			}
			
			DeliveryDocument docAlias = null;
			DeliveryDocumentItem docItemsAlias = null;
			GoodsInRouteListResult resultNodeAlias = null;
			
			result = uow.Session.QueryOver<DeliveryDocument>(() => docAlias)
				.Inner.JoinAlias(d => d.Items, () => docItemsAlias)
				.WhereRestrictionOn(d => d.RouteListItem.Id).IsIn(routeList.Addresses.Select(x => x.Id).ToArray())
				.And(() => docItemsAlias.Direction == DeliveryDirection.ToClient)
				.SelectList(list => list
					.SelectGroup(() => docItemsAlias.Nomenclature.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
					.SelectSum(() => docItemsAlias.Amount).WithAlias(() => resultNodeAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();
			
			return result;
		}

		public IEnumerable<GoodsInRouteListResult> AllGoodsDelivered(IEnumerable<DeliveryDocument> deliveryDocuments)
		{
			return deliveryDocuments.SelectMany(d => d.Items.Where(x => x.Direction == DeliveryDirection.ToClient))
				.GroupBy(i => i.Nomenclature.Id,
					(nomenclatureId, items) =>
						new GoodsInRouteListResult {
							NomenclatureId = nomenclatureId,
							Amount = items.Sum(x => x.Amount)
						});
		}
		
		public IEnumerable<GoodsInRouteListResult> AllGoodsReceivedFromClient(IEnumerable<DeliveryDocument> deliveryDocuments)
		{
			return deliveryDocuments.SelectMany(d => d.Items.Where(x => x.Direction == DeliveryDirection.FromClient))
				.GroupBy(i => i.Nomenclature.Id,
					(nomenclatureId, items) =>
						new GoodsInRouteListResult {
							NomenclatureId = nomenclatureId,
							Amount = items.Sum(x => x.Amount)
						});
		}

		public IEnumerable<GoodsInRouteListResult> AllGoodsTransferredFrom(IUnitOfWork uow, RouteList routeList)
		{
			if(routeList == null) throw new ArgumentNullException(nameof(routeList));

			AddressTransferDocument transferDocAlias = null;
			AddressTransferDocumentItem transferDocItemAlias = null;
			DriverNomenclatureTransferItem driverTransferDocItemAlias = null;
			GoodsInRouteListResult resultNodeAlias = null;
			
			var result = uow.Session.QueryOver<AddressTransferDocument>(() => transferDocAlias)
				.Inner.JoinAlias(() => transferDocAlias.AddressTransferDocumentItems, () => transferDocItemAlias)
				.Inner.JoinAlias(() => transferDocItemAlias.DriverNomenclatureTransferDocumentItems, () => driverTransferDocItemAlias)
				.Where(() => transferDocAlias.RouteListFrom.Id == routeList.Id)
				.SelectList(list => list
					.SelectGroup(() => driverTransferDocItemAlias.Nomenclature.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
					.SelectSum(() => driverTransferDocItemAlias.Amount).WithAlias(() => resultNodeAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<GoodsInRouteListResult>())
				.List<GoodsInRouteListResult>();
			
			return result;
		}

		public IEnumerable<GoodsInRouteListResult> AllGoodsTransferredTo(IUnitOfWork uow, RouteList routeList)
		{
			if(routeList == null) throw new ArgumentNullException(nameof(routeList));

			AddressTransferDocument transferDocAlias = null;
			AddressTransferDocumentItem transferDocItemAlias = null;
			DriverNomenclatureTransferItem driverTransferDocItemAlias = null;
			GoodsInRouteListResult resultNodeAlias = null;
			
			var result = uow.Session.QueryOver<AddressTransferDocument>(() => transferDocAlias)
				.Inner.JoinAlias(() => transferDocAlias.AddressTransferDocumentItems, () => transferDocItemAlias)
				.Inner.JoinAlias(() => transferDocItemAlias.DriverNomenclatureTransferDocumentItems, () => driverTransferDocItemAlias)
				.Where(() => transferDocAlias.RouteListTo.Id == routeList.Id)
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
			WarehouseMovementOperation movementOperationAlias = null;

			var returnableQuery = uow.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.WarehouseMovementOperation, () => movementOperationAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias(() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => !nomenclatureAlias.IsSerial);

			if(categories != null) {
				returnableQuery.Where(() => nomenclatureAlias.Category.IsIn(categories));
			}

			if(excludeNomenclatureIds != null) {
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
			Equipment equipmentAlias = null;
			CarUnloadDocument carUnloadAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;

			var returnableQuery = QueryOver.Of<CarUnloadDocument>(() => carUnloadAlias)
				   .JoinAlias(() => carUnloadAlias.Items, () => carUnloadItemsAlias)
				   .JoinAlias(() => carUnloadItemsAlias.WarehouseMovementOperation, () => movementOperationAlias)
				   .JoinAlias(() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
				   .Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
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
					.Select(() => carUnloadItemsAlias.DefectSource).WithAlias(() => resultAlias.DefectSource)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
			)
			.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
			.List<ReturnsNode>();

			var returnableQueryEquipment = uow.Session.QueryOver<CarUnloadDocument>(() => carUnloadAlias)
				.JoinAlias(() => carUnloadAlias.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.WarehouseMovementOperation, () => movementOperationAlias)
				.JoinAlias(() => movementOperationAlias.Equipment, () => equipmentAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
				.Where(() => carUnloadAlias.RouteList.Id == routeListId)
				.Where(() => nomenclatureAlias.Id.IsIn(nomenclatureIds))
				;
			

			var returnableEquipment =
				returnableQueryEquipment.SelectList(list => list
					.Select(() => equipmentAlias.Id).WithAlias(() => resultAlias.Id)
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.IsSerial).WithAlias(() => resultAlias.Trackable)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Type).WithAlias(() => resultAlias.EquipmentType)
					.Select(() => carUnloadItemsAlias.DefectSource).WithAlias(() => resultAlias.DefectSource)
									  )
				.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
				.List<ReturnsNode>();

			result.AddRange(returnableItems);
			result.AddRange(returnableEquipment);
			DomainHelper.FillPropertyByEntity<ReturnsNode, Nomenclature>(uow, result, x => x.NomenclatureId, (node, nom) => node.Nomenclature = nom);
			return result;
		}

		public int BottlesUnloadedByCarUnloadedDocuments(IUnitOfWork uow, int emptyBottleId, int routeListId, params int[] exceptDocumentIds) {

			WarehouseMovementOperation movementOperationAlias = null;

			var returnableQuery = OrderItemsReturnedToAllWarehouses(uow, routeListId, emptyBottleId).GetExecutableQueryOver(uow.Session);
			var result = returnableQuery.WhereNot(x => x.Id.IsIn(exceptDocumentIds))
										.SelectList(list => list.SelectSum(() => movementOperationAlias.Amount))
										.SingleOrDefault<decimal>()
										;
			return (int)result;
		}

		public QueryOver<CarUnloadDocument> OrderItemsReturnedToAllWarehouses(IUnitOfWork uow, int routeListId, params int[] nomenclatureIds)
		{
			List<ReturnsNode> result = new List<ReturnsNode>();
			Nomenclature nomenclatureAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;
			CarUnloadDocument carUnloadAlias = null;

			

			var returnableQuery = QueryOver.Of<CarUnloadDocument>()
										   .Where(doc => doc.RouteList.Id == routeListId)
										   .JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
										   .JoinAlias(() => carUnloadItemsAlias.WarehouseMovementOperation, () => movementOperationAlias)
										   .Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
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

		public RouteList GetRouteListByOrder(IUnitOfWork uow, Domain.Orders.Order order)
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			return uow.Session.QueryOver(() => routeListAlias)
							  .Left.JoinAlias(() => routeListAlias.Addresses, () => routeListItemAlias)
							  .Where(() => routeListItemAlias.Order.Id == order.Id)
							  .List()
							  .FirstOrDefault();
		}

		public bool RouteListWasChanged(RouteList routeList)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var actualRouteList = uow.GetById<RouteList>(routeList.Id);
				return actualRouteList.Version != routeList.Version;
			}
		}
	}

	#region DTO

	public class ReturnsNode
	{
		public int Id { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }
		public decimal Amount { get; set; }
		public bool Trackable { get; set; }
		public EquipmentType EquipmentType { get; set; }
		public DefectSource DefectSource { get; set; }
		
		public string Serial {
			get {
				if(Trackable)
					return Id > 0 ? Id.ToString() : "(не определен)";
				return string.Empty;
			}
		}

		public bool Returned {
			get => Amount > 0;
			set => Amount = value ? 1 : 0;
		}
	}

	public class GoodsInRouteListResult
	{
		public int NomenclatureId { get; set; }
		public decimal Amount { get; set; }
	}

	public class GoodsInRouteListResultWithSpecialRequirements
	{
		public int NomenclatureId { get; set; }
		public string NomenclatureName { get; set; }
		public decimal? ExpireDatePercent { get; set; } = null;
		public decimal Amount { get; set; }
	}

	#endregion
}