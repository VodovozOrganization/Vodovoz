using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IRouteListItemRepository
	{
		IList<RouteListItem> GetRouteListItemAtDay(IUnitOfWork uow, DateTime date, RouteListItemStatus? status);
		RouteListItem GetRouteListItemForOrder(IUnitOfWork uow, Order order);
		RouteListItem GetTransferedFrom(IUnitOfWork uow, RouteListItem item);
		bool HasRouteListItemsForOrder(IUnitOfWork uow, Order order);
		bool WasOrderInAnyRouteList(IUnitOfWork uow, Order order);
		bool AnotherRouteListItemForOrderExist(IUnitOfWork uow, RouteListItem routeListItem);
		RouteListItem GetRouteListItemById(IUnitOfWork uow, int routeListAddressId);
	}
}
