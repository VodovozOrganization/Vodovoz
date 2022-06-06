using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class RouteListItemRepository : IRouteListItemRepository
	{
		public RouteListItem GetRouteListItemForOrder(IUnitOfWork uow, Domain.Orders.Order order)
		{
			RouteListItem routeListItemAlias = null;

			return uow.Session.QueryOver(() => routeListItemAlias)
					  .Where(rli => rli.Status != RouteListItemStatus.Transfered)
					  .Where(() => routeListItemAlias.Order.Id == order.Id)
					  .SingleOrDefault();
		}
		
		public IList<RouteListItem> GetRouteListItemsForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<RouteListItem>()
				.Where(x => x.Order.Id == orderId)
				.List();
		}

		public RouteListItem GetTransferredRouteListItemFromRouteListForOrder(IUnitOfWork uow, int routeListId, int orderId)
		{
			return uow.Session.QueryOver<RouteListItem>()
				.Where(rla => rla.Status == RouteListItemStatus.Transfered)
				.And(rla => rla.RouteList.Id == routeListId)
				.And(rla => rla.Order.Id == orderId)
				.SingleOrDefault();
		}

		public bool HasRouteListItemsForOrder(IUnitOfWork uow, Domain.Orders.Order order)
		{
			return uow.Session.QueryOver<RouteListItem>()
					  .Where(x => x.Order.Id == order.Id)
					  .Select(Projections.Count<RouteListItem>(x => x.Id))
					  .SingleOrDefault<int>() > 0;
		}

		public bool WasOrderInAnyRouteList(IUnitOfWork uow, Domain.Orders.Order order)
		{
			return !uow.Session.QueryOver<RouteListItem>()
					   .Where(i => i.Order == order)
					   .List()
					   .Any();
		}

		public IList<RouteListItem> GetRouteListItemAtDay(IUnitOfWork uow, DateTime date, RouteListItemStatus? status)
		{
			RouteListItem routeListItemAlias = null;
			RouteList routelistAlias = null;

			var query = uow.Session.QueryOver(() => routeListItemAlias)
				.JoinQueryOver(rli => rli.RouteList, () => routelistAlias)
				.Where(() => routelistAlias.Date == date);
			if(status != null)
				query.Where(() => routeListItemAlias.Status == status.Value);

			return query.List();
		}

		public RouteListItem GetTransferedFrom(IUnitOfWork uow, RouteListItem item)
		{
			if(!item.WasTransfered)
				return null;
			return uow.Session.QueryOver<RouteListItem>()
					  .Where(rli => rli.TransferedTo.Id == item.Id)
					  .Take(1)
					  .SingleOrDefault();
		}

		public bool AnotherRouteListItemForOrderExist(IUnitOfWork uow, RouteListItem routeListItem)
		{
			if(routeListItem.Status == RouteListItemStatus.Transfered)
				return false;
			RouteListItemStatus[] undeliveryStatus = RouteListItem.GetUndeliveryStatuses();
			foreach(var status in undeliveryStatus) {
				if(routeListItem.Status == status)
					return false;
			}

			var anotherRouteListItem = uow.Session.QueryOver<RouteListItem>()
					.Where(x => x.Order.Id == routeListItem.Order.Id)
					.And(x => x.Id != routeListItem.Id)
					.And(x => x.Status != RouteListItemStatus.Transfered)
					.And(!Restrictions.In(Projections.Property<RouteListItem>(x => x.Status), undeliveryStatus))
					.And(x => x.RouteList.Id != routeListItem.RouteList.Id)
					.Take(1).List().FirstOrDefault();
			return anotherRouteListItem != null;
		}

		public bool CurrentRouteListHasOrderDuplicate(IUnitOfWork uow, RouteListItem routeListItem, int [] actualRouteListItemIds)
		{
			if(routeListItem.Status == RouteListItemStatus.Transfered)
			{
				return false;
			}

			var currentRouteListOrderDuplicate = uow.Session.QueryOver<RouteListItem>()
				.Where(x => x.Order.Id == routeListItem.Order.Id)
				.And(x => x.Id != routeListItem.Id)
				.And(x => x.RouteList.Id == routeListItem.RouteList.Id)
				.And(Restrictions.In(Projections.Property<RouteListItem>(x => x.Id), actualRouteListItemIds))
				.And(x => x.Status != RouteListItemStatus.Transfered)
				.Take(1).List().FirstOrDefault();

			return currentRouteListOrderDuplicate != null;
		}

		public RouteListItem GetRouteListItemById(IUnitOfWork uow, int routeListAddressId)
		{
			return uow.GetById<RouteListItem>(routeListAddressId);
		}

		public bool HasEnoughQuantityForFastDelivery(IUnitOfWork uow, RouteListItem routeListItemFrom, RouteList routeListTo)
		{
			RouteListItem routeListItemAlias = null;
			OrderItem orderItemAlias = null;
			NomenclatureAmountNode nomenclatureAmountAlias = null;
			Order orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			CarLoadDocument carLoadDocumentAlias = null;
			CarLoadDocumentItem carLoadDocumentItemAlias = null;

			var nomenclaturesToDeliver = routeListItemFrom.Order.GetAllGoodsToDeliver();

			var neededIds = nomenclaturesToDeliver.Select(x => x.NomenclatureId).ToArray();

			var orderItemsToDeliver = uow.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
				.Inner.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Inner.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Where(() => routeListItemAlias.RouteList.Id == routeListTo.Id)
				.WhereRestrictionOn(() => orderItemAlias.Nomenclature.Id).IsIn(neededIds)
				.WhereRestrictionOn(() => routeListItemAlias.Status).Not.IsIn(new RouteListItemStatus[] { RouteListItemStatus.Canceled, RouteListItemStatus.Overdue, RouteListItemStatus.Transfered })
				.SelectList(list => list
					.SelectGroup(() => orderItemAlias.Nomenclature.Id).WithAlias(() => nomenclatureAmountAlias.NomenclatureId)
					.SelectSum(() => orderItemAlias.Count).WithAlias(() => nomenclatureAmountAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<NomenclatureAmountNode>())
				.Future<NomenclatureAmountNode>();

			var orderEquipmentsToDeliver = uow.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
				.Inner.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Inner.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.Where(() => routeListItemAlias.RouteList.Id == routeListTo.Id)
				.WhereRestrictionOn(() => orderEquipmentAlias.Nomenclature.Id).IsIn(neededIds)
				.WhereRestrictionOn(() => routeListItemAlias.Status).Not.IsIn(new RouteListItemStatus[] { RouteListItemStatus.Canceled, RouteListItemStatus.Overdue, RouteListItemStatus.Transfered })
				.And(() => orderEquipmentAlias.Direction == Domain.Orders.Direction.Deliver)
				.SelectList(list => list
					.SelectGroup(() => orderEquipmentAlias.Nomenclature.Id).WithAlias(() => nomenclatureAmountAlias.NomenclatureId)
					.Select(Projections.Cast(NHibernateUtil.Decimal, Projections.Sum(Projections.Property(() => orderEquipmentAlias.Count)))).WithAlias(() => nomenclatureAmountAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<NomenclatureAmountNode>())
				.Future<NomenclatureAmountNode>();

			var allToDeliver = orderItemsToDeliver
			.Union(orderEquipmentsToDeliver)
			.GroupBy(x => new { x.NomenclatureId })
			.Select(group => new NomenclatureAmountNode()
			{
				NomenclatureId = group.Key.NomenclatureId,
				Amount = group.Sum(x => x.Amount)
			})
			.ToList();

			var allLoaded = uow.Session.QueryOver<CarLoadDocument>(() => carLoadDocumentAlias)
				.Inner.JoinAlias(() => carLoadDocumentAlias.Items, () => carLoadDocumentItemAlias)
				.Where(() => carLoadDocumentAlias.RouteList.Id == routeListTo.Id)
				.WhereRestrictionOn(() => carLoadDocumentItemAlias.Nomenclature.Id).IsIn(neededIds)
				.SelectList(list => list
					.SelectGroup(() => carLoadDocumentItemAlias.Nomenclature.Id).WithAlias(() => nomenclatureAmountAlias.NomenclatureId)
					.SelectSum(() => carLoadDocumentItemAlias.Amount).WithAlias(() => nomenclatureAmountAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<NomenclatureAmountNode>())
			.List<NomenclatureAmountNode>();

			foreach(var need in nomenclaturesToDeliver)
			{
				var toDeliver = allToDeliver.FirstOrDefault(x => x.NomenclatureId == need.NomenclatureId)?.Amount ?? 0;
				var loaded = allLoaded.FirstOrDefault(x => x.NomenclatureId == need.NomenclatureId)?.Amount ?? 0;

				if(loaded - toDeliver < need.Amount)
				{
					return false;
				}
			}

			return true;
		}
	}
}
