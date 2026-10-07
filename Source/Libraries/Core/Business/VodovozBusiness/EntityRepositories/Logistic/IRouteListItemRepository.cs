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
		IList<RouteListItem> GetRouteListItemsForOrder(IUnitOfWork uow, int orderId);
		RouteListItem GetTransferredRouteListItemFromRouteListForOrder(IUnitOfWork uow, int routeListId, int orderId);
		RouteListItem GetTransferredFrom(IUnitOfWork uow, RouteListItem item);
		RouteListItem GetTransferredTo(IUnitOfWork uow, RouteListItem item);
		AddressTransferType? GetAddressTransferType(IUnitOfWork uow, int oldAddressId, int newAddressId);
		bool HasRouteListItemsForOrder(IUnitOfWork uow, Order order);
		bool WasOrderInAnyRouteList(IUnitOfWork uow, Order order);
		bool AnotherRouteListItemForOrderExist(IUnitOfWork uow, RouteListItem routeListItem);
		bool CurrentRouteListHasOrderDuplicate(IUnitOfWork uow, RouteListItem routeListItem, int[] actualRouteListItemIds);
		RouteListItem GetRouteListItemById(IUnitOfWork uow, int routeListAddressId);
		string GetUnscannedCodesReason(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Проверяет, были ли изменения в переданных адресах по сравнению с их состоянием в базе данных.
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="routeListItems">Список адресов для проверки</param>
		/// <returns>Возвращает true, если были изменения, иначе false</returns>
		bool RouteListItemWasChanged(IUnitOfWork uow, IEnumerable<RouteListItem> routeListItems);
	}
}
