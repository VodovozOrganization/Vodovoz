using System;
using Vodovoz.Domain.Logistic;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using System.Collections.Generic;

namespace Vodovoz.Repository.Logistics
{
	public static class RouteListItemRepository
	{
		public static RouteListItem GetRouteListItemForOrder(IUnitOfWork uow, Order order)
		{
			RouteListItem routeListItemAlias = null;

			return uow.Session.QueryOver<RouteListItem> (() => routeListItemAlias)
				.Where (() => routeListItemAlias.Order == order)
				.SingleOrDefault ();
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

	}
}

