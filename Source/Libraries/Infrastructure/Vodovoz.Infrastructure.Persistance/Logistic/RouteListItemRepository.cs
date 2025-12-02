using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class RouteListItemRepository : IRouteListItemRepository
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

		public RouteListItem GetTransferredFrom(IUnitOfWork uow, RouteListItem item)
		{
			RouteListItem routeListItemAlias = null;
			AddressTransferDocumentItem addressTransferDocumentItemAlias = null;

			if(!item.WasTransfered)
			{
				return null;
			}

			return uow.Session.QueryOver(() => routeListItemAlias)
				.JoinEntityAlias(() => addressTransferDocumentItemAlias, () => routeListItemAlias.Id == addressTransferDocumentItemAlias.OldAddress.Id)
				.Where(() => addressTransferDocumentItemAlias.NewAddress.Id == item.Id)
				.OrderBy(() => addressTransferDocumentItemAlias.Id).Desc
				.Take(1)
				.SingleOrDefault();
		}

		public RouteListItem GetTransferredTo(IUnitOfWork uow, RouteListItem item)
		{
			RouteListItem routeListItemAlias = null;
			AddressTransferDocumentItem addressTransferDocumentItemAlias = null;

			if(item.TransferedTo == null)
			{
				return null;
			}

			return uow.Session.QueryOver(() => routeListItemAlias)
				.JoinEntityAlias(() => addressTransferDocumentItemAlias, () => routeListItemAlias.Id == addressTransferDocumentItemAlias.NewAddress.Id)
				.Where(() => addressTransferDocumentItemAlias.OldAddress.Id == item.Id)
				.OrderBy(() => addressTransferDocumentItemAlias.Id).Desc
				.Take(1)
				.SingleOrDefault();
		}

		public AddressTransferType? GetAddressTransferType(IUnitOfWork uow, int oldAddressId, int newAddressId)
		{
			AddressTransferDocumentItem addressTransferDocumentItemAlias = null;

			var result = uow.Session.QueryOver(() => addressTransferDocumentItemAlias)
				.Where(() => addressTransferDocumentItemAlias.OldAddress.Id == oldAddressId)
				.And(() => addressTransferDocumentItemAlias.NewAddress.Id == newAddressId)
				.OrderBy(() => addressTransferDocumentItemAlias.Id).Desc
				.Take(1)
				.SingleOrDefault();

			return result?.AddressTransferType;
		}

		public bool AnotherRouteListItemForOrderExist(IUnitOfWork uow, RouteListItem routeListItem)
		{
			if(routeListItem.Status == RouteListItemStatus.Transfered)
				return false;
			RouteListItemStatus[] undeliveryStatus = RouteListItem.GetUndeliveryStatuses();
			foreach(var status in undeliveryStatus)
			{
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

		public bool CurrentRouteListHasOrderDuplicate(IUnitOfWork uow, RouteListItem routeListItem, int[] actualRouteListItemIds)
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

		public string GetUnscannedCodesReason(IUnitOfWork uow, int orderId)
		{
			return (
				from routeListAddress in uow.Session.Query<RouteListItem>()
				where routeListAddress.Order.Id == orderId
					&& routeListAddress.Status != RouteListItemStatus.Transfered
				select routeListAddress.UnscannedCodesReason
				)
				.FirstOrDefault();
		}
	}
}
