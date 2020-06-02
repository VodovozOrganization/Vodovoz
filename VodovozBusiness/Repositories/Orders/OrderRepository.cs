using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Orders;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repositories.Orders
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Orders")]
	public static class OrderRepository
	{
		public static QueryOver<VodovozOrder> GetSelfDeliveryOrdersForPaymentQuery()
		{
			return OrderSingletonRepository.GetInstance().GetSelfDeliveryOrdersForPaymentQuery();
		}

		public static QueryOver<VodovozOrder> GetOrdersForRLEditingQuery(DateTime date, bool showShipped)
		{
			return OrderSingletonRepository.GetInstance().GetOrdersForRLEditingQuery(date, showShipped);
		}

		public static IList<VodovozOrder> GetAcceptedOrdersForRegion(IUnitOfWork uow, DateTime date, District district)
		{
			return OrderSingletonRepository.GetInstance().GetAcceptedOrdersForRegion(uow, date, district);
		}

		public static VodovozOrder GetLatestCompleteOrderForCounterparty(IUnitOfWork UoW, Counterparty counterparty)
		{
			return OrderSingletonRepository.GetInstance().GetLatestCompleteOrderForCounterparty(UoW, counterparty);
		}

		public static IList<VodovozOrder> GetCurrentOrders(IUnitOfWork UoW, Counterparty counterparty)
		{
			return OrderSingletonRepository.GetInstance().GetCurrentOrders(UoW, counterparty);
		}

		public static IList<VodovozOrder> GetOrdersToExport1c8(IUnitOfWork UoW, Export1cMode mode, DateTime startDate, DateTime endDate)
		{
			return OrderSingletonRepository.GetInstance().GetOrdersToExport1c8(UoW, mode, startDate, endDate);
		}

		public static IList<VodovozOrder> GetOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate)
		{
			return OrderSingletonRepository.GetInstance().GetOrdersBetweenDates(UoW, startDate, endDate);
		}

		public static IList<VodovozOrder> GetOrdersByCode1c(IUnitOfWork uow, string[] codes1c)
		{
			return OrderSingletonRepository.GetInstance().GetOrdersByCode1c(uow, codes1c);
		}

		internal static Func<IUnitOfWork, Counterparty, VodovozOrder> GetFirstRealOrderForClientTestGap;
		/// <summary>
		/// Первый заказ контрагента, который можно считать выполненым.
		/// </summary>
		/// <returns>Первый заказ</returns>
		/// <param name="uow">UoW</param>
		/// <param name="counterparty">Контрагент</param>
		public static VodovozOrder GetFirstRealOrderForClientForActionBottle(IUnitOfWork uow, VodovozOrder order,Counterparty counterparty)
		{
			return OrderSingletonRepository.GetInstance().GetFirstRealOrderForClientForActionBottle(uow, order, counterparty);
		}

		/// <summary>
		/// Кол-во 19л. воды в заказе
		/// </summary>
		/// <returns>Кол-во 19л. воды в заказе</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		public static int Get19LWatterQtyForOrder(IUnitOfWork uow, VodovozOrder order)
		{
			return OrderSingletonRepository.GetInstance().Get19LWatterQtyForOrder(uow, order);
		}

		/// <summary>
		/// Оборудование заказа к клиенту
		/// </summary>
		/// <returns>Список оборудования к клиенту</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		public static IList<ClientEquipmentNode> GetEquipmentToClientForOrder(IUnitOfWork uow, VodovozOrder order)
		{
			return OrderSingletonRepository.GetInstance().GetEquipmentToClientForOrder(uow, order);
		}

		/// <summary>
		/// Оборудование заказа от клиента
		/// </summary>
		/// <returns>Список оборудования от клиенту</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		public static IList<ClientEquipmentNode> GetEquipmentFromClientForOrder(IUnitOfWork uow, VodovozOrder order)
		{
			return OrderSingletonRepository.GetInstance().GetEquipmentFromClientForOrder(uow, order);
		}

		/// <summary>
		/// Список последних заказов для точки доставки.
		/// </summary>
		/// <returns>Список последних заказов для точки доставки.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="deliveryPoint">Точка доставки.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		public static IList<VodovozOrder> GetLatestOrdersForDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int? count = null)
		{
			return OrderSingletonRepository.GetInstance().GetLatestOrdersForDeliveryPoint(UoW, deliveryPoint, count);
		}

		/// <summary>
		/// Список МЛ для заказа, отсортированный в порядке владения этим заказом, в случае переносов
		/// </summary>
		/// <returns>Список МЛ</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="order">Заказ</param>
		public static IList<RouteList> GetAllRLForOrder(IUnitOfWork UoW, VodovozOrder order)
		{
			return OrderSingletonRepository.GetInstance().GetAllRLForOrder(UoW, order);
		}

		/// <summary>
		/// Возврат отсортированного списка скидок
		/// </summary>
		/// <returns>Список скидок</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		public static IList<DiscountReason> GetDiscountReasons(IUnitOfWork UoW, bool orderByDescending = false)
		{
			return OrderSingletonRepository.GetInstance().GetDiscountReasons(UoW, orderByDescending);
		}

		public static VodovozOrder GetOrderOnDateAndDeliveryPoint(IUnitOfWork uow, DateTime date, DeliveryPoint deliveryPoint)
		{
			return OrderSingletonRepository.GetInstance().GetOrderOnDateAndDeliveryPoint(uow, date, deliveryPoint);
		}

		public static bool IsBottleStockExists(IUnitOfWork uow, Counterparty counterparty)
		{
			return OrderSingletonRepository.GetInstance().IsBottleStockExists(uow, counterparty);
		}

		public static OrderStatus[] GetOnClosingOrderStatuses()
		{
			return OrderSingletonRepository.GetInstance().GetOnClosingOrderStatuses();
		}

		public static OrderStatus[] GetStatusesForOrderCancelation()
		{
			return OrderSingletonRepository.GetInstance().GetStatusesForOrderCancelation();
		}

		public static OrderStatus[] GetStatusesForActualCount(VodovozOrder order)
		{
			return OrderSingletonRepository.GetInstance().GetStatusesForActualCount(order);
		}

		public static OrderStatus[] GetGrantedStatusesToCreateSeveralOrders()
		{
			return OrderSingletonRepository.GetInstance().GetGrantedStatusesToCreateSeveralOrders();
		}

		public static OrderStatus[] GetValidStatusesToUseActionBottle()
		{
			return OrderSingletonRepository.GetInstance().GetValidStatusesToUseActionBottle();
		}
	}
}