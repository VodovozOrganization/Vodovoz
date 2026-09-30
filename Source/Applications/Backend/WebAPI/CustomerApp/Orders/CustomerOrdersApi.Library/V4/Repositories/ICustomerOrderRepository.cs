using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V4.Repositories
{
	public interface ICustomerOrderRepository
	{
		/// <summary>
		/// Получение заказов контрагента, которые связаны с онлайн-заказами
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, с которой доступна рейтинговая информация</param>
		/// <returns>Список заказов</returns>
		IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom
			);

		/// <summary>
		/// Получение заказов контрагента, которые не связаны с онлайн-заказами
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, с которой доступна рейтинговая информация</param>
		/// <returns>Список заказов</returns>
		IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom
			);

		/// <summary>
		/// Возвращает онлайн-заказы контрагента, для которых ещё не создан заказ в системе
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, начиная с которой доступна оценка заказа</param>
		/// <returns>Коллекция DTO заказов</returns>
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom
			);
	}
}
