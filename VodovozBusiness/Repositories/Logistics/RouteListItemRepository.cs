using System;
using System.Collections.Generic;
using NHibernate.Util;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Repository.Logistics
{
	public static class RouteListItemRepository
	{
		public static RouteListItem GetRouteListItemForOrder(IUnitOfWork uow, Order order)
		{
			RouteListItem routeListItemAlias = null;

			return uow.Session.QueryOver<RouteListItem> (() => routeListItemAlias)
				      .Where(rli => rli.Status != RouteListItemStatus.Transfered)
					  .Where (() => routeListItemAlias.Order == order)
				      .SingleOrDefault ();
		}

		public static bool WasOrderInAnyRouteList(IUnitOfWork uow, Order order)
		{
			return !uow.Session.QueryOver<RouteListItem>()
				       .Where(i => i.Order == order)
					   .List()
					   .Any();
		}

		public static IList<RouteListItem> GetRouteListItemAtDay(IUnitOfWork uow, DateTime date, RouteListItemStatus? status)
		{
			RouteListItem routeListItemAlias = null;
			RouteList routelistAlias = null;

			var query = uow.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
				.JoinQueryOver(rli => rli.RouteList, () => routelistAlias)
				.Where(() => routelistAlias.Date == date);
			if (status != null)
				query.Where(() => routeListItemAlias.Status == status.Value);

			return query.List();
		}

		public static RouteListItem GetTransferedFrom(IUnitOfWork uow, RouteListItem item)
		{
			if (!item.WasTransfered)
				return null;
			return uow.Session.QueryOver<RouteListItem> ()
					  .Where (rli => rli.TransferedTo.Id == item.Id)
					  .Take (1)
					  .SingleOrDefault ();
		}
	}
}

