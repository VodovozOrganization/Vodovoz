using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using System.Linq;
using NHibernate.Dialect.Function;
using NHibernate;
using NHibernate.Transform;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
	public class WarehouseRepository : IWarehouseRepository
	{
		public IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Warehouse>().WhereNot(x => x.IsArchive).List<Warehouse>();
		}

		public IList<Warehouse> WarehouseForShipment(IUnitOfWork uow, int routeListId)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature orderItemNomenclatureAlias = null,
				orderEquipmentNomenclatureAlias = null,
				resultNomenclatureAlias = null;
			Warehouse warehouseAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<Vodovoz.Domain.Logistic.RouteListItem>()
				.Where(r => r.RouteList.Id == routeListId)
				.Where(x => x.WasTransfered == false || (x.WasTransfered && x.NeedToReload))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderitemsSubqury = QueryOver.Of<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => orderItemNomenclatureAlias)
				.Select(n => n.Nomenclature.Id)
				.Where(() => orderItemNomenclatureAlias.NoDelivey == false);
			var orderEquipmentSubquery = QueryOver.Of<OrderEquipment>(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderEquipmentAlias.Order, () => orderAlias)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias(e => e.Nomenclature, () => orderEquipmentNomenclatureAlias)
				.SelectList(list => list.Select(() => orderEquipmentNomenclatureAlias.Id));

			var warehouses = uow.Session.QueryOver<Nomenclature>(() => resultNomenclatureAlias)
				.Where(new Disjunction()
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderitemsSubqury))
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderEquipmentSubquery))
				)
				.JoinAlias(() => resultNomenclatureAlias.Warehouses, () => warehouseAlias)
				.Select(Projections.Distinct(Projections.Entity<Warehouse>(() => warehouseAlias)))
				.List<Warehouse>();

			return warehouses;
		}

		public IList<Warehouse> WarehouseForReception(IUnitOfWork uow, int id)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null, orderNewEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null, OrderNewEquipmentNomenclatureAlias = null, resultNomenclatureAlias = null;
			Warehouse warehouseAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<Vodovoz.Domain.Logistic.RouteListItem>()
				.Where(r => r.RouteList.Id == id)
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderitemsSubqury = QueryOver.Of<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Select(n => n.Nomenclature.Id);
			var orderEquipmentSubquery = QueryOver.Of<OrderEquipment>(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderEquipmentAlias.Order, () => orderAlias)
				.JoinQueryOver(() => orderEquipmentAlias.Equipment)
				.JoinAlias(e => e.Nomenclature, () => OrderEquipmentNomenclatureAlias)
				.SelectList(list => list.Select(() => OrderEquipmentNomenclatureAlias.Id));
			var orderNewEquipmentSubquery = QueryOver.Of<OrderEquipment>(() => orderNewEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderNewEquipmentAlias.Order, () => orderAlias)
				.JoinAlias(() => orderNewEquipmentAlias.Nomenclature, () => OrderNewEquipmentNomenclatureAlias)
				.SelectList(list => list.Select(() => OrderNewEquipmentNomenclatureAlias.Id));

			var warehouses = uow.Session.QueryOver<Nomenclature>(() => resultNomenclatureAlias)
				.Where(new Disjunction()
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderitemsSubqury))
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderEquipmentSubquery))
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderNewEquipmentSubquery))
				)
				.JoinAlias(() => resultNomenclatureAlias.Warehouses, () => warehouseAlias)
				.Select(Projections.Distinct(Projections.Entity<Warehouse>(() => warehouseAlias)))
				.List<Warehouse>();

			return warehouses;
		}

		public IList<Warehouse> WarehousesForPublishOnlineStore(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Warehouse>()
					  .WhereNot(x => x.IsArchive)
					  .Where(x => x.PublishOnlineStore)
					  .List<Warehouse>();
		}

		public IEnumerable<NomanclatureStockNode> GetWarehouseNomenclatureStock(IUnitOfWork uow, int warehouseId, IEnumerable<int> nomenclatureIds)
		{
			NomanclatureStockNode resultAlias = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation warehouseOperation = null;

			IProjection incomeAmount = Projections.Sum(
				Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => warehouseOperation.IncomingWarehouse.Id), warehouseId),
					Projections.Property(() => warehouseOperation.Amount),
					Projections.Constant(0M)
				)
			);

			IProjection writeoffAmount = Projections.Sum(
				Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => warehouseOperation.WriteoffWarehouse.Id), warehouseId),
					Projections.Property(() => warehouseOperation.Amount),
					Projections.Constant(0M)
				)
			);

			IProjection stockProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
					NHibernateUtil.Int32,
					incomeAmount,
					writeoffAmount
			);

			return uow.Session.QueryOver(() => warehouseOperation)
				.Left.JoinAlias(() => warehouseOperation.Nomenclature, () => nomenclatureAlias)
				.Where(Restrictions.In(Projections.Property(() => warehouseOperation.Nomenclature.Id), nomenclatureIds.ToArray()))
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(stockProjection).WithAlias(() => resultAlias.Stock)
				)
				.TransformUsing(Transformers.AliasToBean<NomanclatureStockNode>())
				.List<NomanclatureStockNode>();
		}

		public IEnumerable<NomanclatureStockNode> GetWarehouseNomenclatureStock(IUnitOfWork uow, int warehouseId)
		{
			NomanclatureStockNode resultAlias = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation warehouseOperation = null;

			IProjection incomeAmount = Projections.Sum(
				Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => warehouseOperation.IncomingWarehouse.Id), warehouseId),
					Projections.Property(() => warehouseOperation.Amount),
					Projections.Constant(0M)
				)
			);

			IProjection writeoffAmount = Projections.Sum(
				Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => warehouseOperation.WriteoffWarehouse.Id), warehouseId),
					Projections.Property(() => warehouseOperation.Amount),
					Projections.Constant(0M)
				)
			);

			IProjection stockProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
					NHibernateUtil.Int32,
					incomeAmount,
					writeoffAmount
			);

			return uow.Session.QueryOver(() => warehouseOperation)
				.Left.JoinAlias(() => warehouseOperation.Nomenclature, () => nomenclatureAlias)
				.SelectList(list => list
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(stockProjection).WithAlias(() => resultAlias.Stock)
				)
				.TransformUsing(Transformers.AliasToBean<NomanclatureStockNode>())
				.List<NomanclatureStockNode>();
		}

		public IEnumerable<Nomenclature> GetDiscrepancyNomenclatures(IUnitOfWork uow, int warehouseId)
		{
			if(uow == null) {
				throw new ArgumentNullException(nameof(uow));
			}

			Nomenclature nomenclatureAlias = null;
			MovementDocument movementDocumentAlias = null;
			MovementDocumentItem movementDocumentItemAlias = null;

			return uow.Session.QueryOver(() => movementDocumentItemAlias)
				.Left.JoinAlias(() => movementDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => movementDocumentItemAlias.Document, () => movementDocumentAlias)
				.Where(() => movementDocumentAlias.Status == MovementDocumentStatus.Discrepancy)
				.Where(() => movementDocumentAlias.FromWarehouse.Id == warehouseId)
				.Where(() => movementDocumentItemAlias.SendedAmount != movementDocumentItemAlias.ReceivedAmount)
				.Select(Projections.Entity(() => nomenclatureAlias))
				.List<Nomenclature>();
		}
	}
}
