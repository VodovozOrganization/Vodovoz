using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Repositories
{
	public interface ICustomerOrderRepository
	{
		/// <summary>
		/// Получение заказов контрагента, которые связаны с онлайн-заказами
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, с которой доступна рейтинговая информация</param>
		/// <param name="orderStatuses">Статусы заказов</param>
		/// <returns>Список заказов</returns>
		IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom,
			IEnumerable<ExternalOrderStatus> orderStatuses = null
			);

		/// <summary>
		/// Получение заказов контрагента, которые не связаны с онлайн-заказами
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, с которой доступна рейтинговая информация</param>
		/// <param name="orderStatuses">Статусы заказов</param>
		/// <returns>Список заказов</returns>
		IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom,
			IEnumerable<ExternalOrderStatus> orderStatuses = null
			);

		/// <summary>
		/// Возвращает онлайн-заказы контрагента, для которых ещё не создан заказ в системе
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, начиная с которой доступна оценка заказа</param>
		/// <param name="orderStatuses">Необязательный список допустимых внешних статусов заказа для фильтрации.
		/// Если не указан — возвращаются заказы во всех статусах</param>
		/// <returns>Коллекция DTO заказов</returns>
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom,
			IEnumerable<ExternalOrderStatus> orderStatuses = null);
	}
}
