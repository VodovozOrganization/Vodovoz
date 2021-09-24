using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

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
					.Take(1).List().FirstOrDefault();
			return anotherRouteListItem != null;
		}

        public RouteListItem GetRouteListItemById(IUnitOfWork uow, int routeListAddressId)
        {
			return uow.GetById<RouteListItem>(routeListAddressId);
        }
	}
}
