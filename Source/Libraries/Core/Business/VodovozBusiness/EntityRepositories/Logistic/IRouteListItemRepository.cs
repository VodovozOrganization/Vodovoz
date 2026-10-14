using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
		/// Получает маршрутные листы адресов по id адресов
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="routeListItemIds">Id адресов маршрутных листов</param>
		/// <returns>Словарь: id адреса - маршрутный лист адреса</returns>
		IDictionary<int, RouteList> GetRouteListsByRouteListItemIds(IUnitOfWork uow, IEnumerable<int> routeListItemIds);

		/// <summary>
		/// Проверяет, были ли изменения в переданных адресах по сравнению с их состоянием в базе данных.
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="routeListItems">Список адресов для проверки</param>
		/// <returns>Возвращает true, если были изменения, иначе false</returns>
		bool RouteListItemWasChanged(IUnitOfWork uow, IEnumerable<RouteListItem> routeListItems);

		/// <summary>
		/// Количество адресов МЛ в статусе «Выполнен» за период дат МЛ (обе даты включительно)
		/// в разрезе водителя МЛ и текущего района точки доставки заказа
		/// с учетом условий по водителям и районам
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="startDate">Начало периода</param>
		/// <param name="endDate">Конец периода</param>
		/// <param name="filter">Условия по водителям и районам; null - без ограничений</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Узлы: водитель, район (может быть пустым), количество адресов</returns>
		Task<IList<CompletedAddressesCountNode>> GetCompletedAddressesCountsByDriverAndDistrictAsync(
			IUnitOfWork uow, DateTime startDate, DateTime endDate, CompletedAddressesCountsFilter filter, CancellationToken cancellationToken);

		/// <summary>
		/// Синхронный вариант <see cref="GetCompletedAddressesCountsByDriverAndDistrictAsync"/> (тот же запрос без условий по водителям и районам)
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="startDate">Начало периода</param>
		/// <param name="endDate">Конец периода</param>
		/// <returns>Узлы: водитель, район (может быть пустым), количество адресов</returns>
		IList<CompletedAddressesCountNode> GetCompletedAddressesCountsByDriverAndDistrict(
			IUnitOfWork uow, DateTime startDate, DateTime endDate);
	}
}
