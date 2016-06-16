using System;
using Vodovoz.Domain.Logistic;
using QSOrmProject;
using Vodovoz.Domain.Orders;

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
	}
}

