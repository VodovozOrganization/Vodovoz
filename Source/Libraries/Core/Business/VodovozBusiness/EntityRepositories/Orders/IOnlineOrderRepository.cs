using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.Default;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IOnlineOrderRepository
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		IEnumerable<Vodovoz.Core.Data.Orders.V4.OrderDto> GetCounterpartyOnlineOrdersWithoutOrderV4(
			IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);

		/// <summary>
		/// Возвращает онлайн-заказы контрагента, для которых ещё не создан заказ в системе
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, начиная с которой доступна оценка заказа</param>
		/// <param name="orderStatuses">Необязательный список допустимых внешних статусов заказа для фильтрации.
		/// Если не указан — возвращаются заказы во всех статусах</param>
		/// <returns>Коллекция DTO заказов</returns>
		IEnumerable<Core.Data.Orders.V6.OrderDto> GetCounterpartyOnlineOrdersWithoutOrderV6(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom,
			IEnumerable<ExternalOrderStatus> orderStatuses = null);

		OnlineOrder GetOnlineOrderByExternalId(IUnitOfWork uow, Guid externalId);
		IEnumerable<OnlineOrder> GetOnlineOrdersDuplicates(IUnitOfWork uow, OnlineOrder currentOnlineOrder, DateTime? createdAt = null);
		OnlineOrder GetOnlineOrderById(IUnitOfWork uow, int onlineOrderId);
		IEnumerable<OnlineOrder> GetWaitingForPaymentOnlineOrders(IUnitOfWork uow);
	}
}
