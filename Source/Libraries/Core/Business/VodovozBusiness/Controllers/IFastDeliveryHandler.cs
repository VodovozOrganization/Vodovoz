﻿using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Controllers
{
	/// <summary>
	/// Контракт обработчика для быстрой доставки 
	/// </summary>
	public interface IFastDeliveryHandler
	{
		/// <summary>
		/// МЛ для добавления быстрой доставки
		/// </summary>
		RouteList RouteListToAddFastDeliveryOrder { get; }
		/// <summary>
		/// Получение истории доступности быстрой доставки
		/// </summary>
		FastDeliveryAvailabilityHistory FastDeliveryAvailabilityHistory { get; }
		/// <summary>
		/// Проверка доступности быстрой доставки
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="order">Заказ</param>
		/// <returns>Результат проверки</returns>
		Result CheckFastDelivery(IUnitOfWork uow, Order order);
		/// <summary>
		/// Добавить заказ в МЛ и уведомить водителя
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="order">Заказ</param>
		/// <param name="callTaskWorker">Обработчик для создания внутренних заявок на звонок</param>
		void TryAddOrderToRouteListAndNotifyDriver(IUnitOfWork uow, Order order, ICallTaskWorker callTaskWorker);
		void NotifyDriverOfFastDeliveryOrderAdded(int orderId);
	}
}
