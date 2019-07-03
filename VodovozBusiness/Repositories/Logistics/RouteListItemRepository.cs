using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Repository.Logistics
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Logistic")]
	public static class RouteListItemRepository
	{
		[Obsolete]
		public static RouteListItem GetRouteListItemForOrder(IUnitOfWork uow, Order order)
		{
			return new EntityRepositories.Logistic.RouteListItemRepository().GetRouteListItemForOrder(uow, order);
		}

		[Obsolete]
		public static bool HasRouteListItemsForOrder(IUnitOfWork uow, Order order)
		{
			return new EntityRepositories.Logistic.RouteListItemRepository().HasRouteListItemsForOrder(uow, order);
		}

		[Obsolete]
		public static bool WasOrderInAnyRouteList(IUnitOfWork uow, Order order)
		{
			return new EntityRepositories.Logistic.RouteListItemRepository().WasOrderInAnyRouteList(uow, order);
		}

		[Obsolete]
		public static IList<RouteListItem> GetRouteListItemAtDay(IUnitOfWork uow, DateTime date, RouteListItemStatus? status)
		{
			return new EntityRepositories.Logistic.RouteListItemRepository().GetRouteListItemAtDay(uow, date, status);
		}

		[Obsolete]
		public static RouteListItem GetTransferedFrom(IUnitOfWork uow, RouteListItem item)
		{
			return new EntityRepositories.Logistic.RouteListItemRepository().GetTransferedFrom(uow, item);
		}
	}
}