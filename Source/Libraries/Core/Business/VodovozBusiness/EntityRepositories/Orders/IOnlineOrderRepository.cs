using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IOnlineOrderRepository
	{
		OnlineOrder GetOnlineOrderByExternalId(IUnitOfWork uow, Guid externalId);
		IEnumerable<OnlineOrder> GetOnlineOrdersDuplicates(IUnitOfWork uow, OnlineOrder currentOnlineOrder, DateTime? createdAt = null);
		OnlineOrder GetOnlineOrderById(IUnitOfWork uow, int onlineOrderId);
		IEnumerable<OnlineOrder> GetWaitingForPaymentOnlineOrders(IUnitOfWork uow);
		/// <summary>
		/// Проверка существования онлайн заказа в БД
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOderId">Идентификатор онлайн заказа</param>
		/// <returns>true - существует, false - нет</returns>
		bool OnlineOrderExists(IUnitOfWork uow, int onlineOderId);
		/// <summary>
		/// Проверяет, что онлайн заказ принадлежит контрагенту
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderId">Идентификатор онлайн заказа</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <returns></returns>
		bool OnlineOrderFromCounterparty(IUnitOfWork uow, int onlineOrderId, int? counterpartyId);
		/// <summary>
		/// Включен автозаказ у онлайн заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderId">Идентификатор онлайн заказа</param>
		/// <returns></returns>
		bool IsAutoOrderEnabled(IUnitOfWork uow, int onlineOrderId);
	}
}
