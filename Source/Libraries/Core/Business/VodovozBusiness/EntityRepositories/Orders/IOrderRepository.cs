using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Payments;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IOrderRepository
	{
		/// <summary>
		/// Кол-во 19л. воды в заказе
		/// </summary>
		/// <returns>Кол-во 19л. воды в заказе</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		int Get19LWatterQtyForOrder(IUnitOfWork uow, Order order);

		IList<Order> GetAcceptedOrdersForRegion(IUnitOfWork uow, DateTime date, int districtId);

		/// <summary>
		/// Список МЛ для заказа, отсортированный в порядке владения этим заказом, в случае переносов
		/// </summary>
		/// <returns>Список МЛ</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="order">Заказ</param>
		IList<RouteList> GetAllRLForOrder(IUnitOfWork UoW, Order order);

		Dictionary<int, IEnumerable<int>> GetAllRouteListsForOrders(IUnitOfWork UoW, IEnumerable<Order> orders);
		Dictionary<int, IEnumerable<int>> GetAllRouteListsForOrders(IUnitOfWork UoW, IEnumerable<int> orders);

		IList<Order> GetCurrentOrders(IUnitOfWork UoW, Counterparty counterparty);

		/// <summary>
		/// Оборудование заказа от клиента
		/// </summary>
		/// <returns>Список оборудования от клиенту</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		IList<ClientEquipmentNode> GetEquipmentFromClientForOrder(IUnitOfWork uow, Order order);

		IList<ClientEquipmentNode> GetEquipmentToClientForOrder(IUnitOfWork uow, Order order);

		/// <summary>
		/// Первый заказ контрагента, который можно считать выполненым.
		/// </summary>
		/// <returns>Первый заказ</returns>
		/// <param name="uow">UoW</param>
		/// <param name="client">Контрагент</param>
		Order GetFirstRealOrderForClientForActionBottle(IUnitOfWork uow, Order order, Counterparty client);

		bool HasCounterpartyFirstRealOrder(IUnitOfWork uow, Counterparty counterparty);
		bool HasCounterpartyOtherFirstRealOrder(IUnitOfWork uow, Counterparty counterparty, int orderId);

		OrderStatus[] GetGrantedStatusesToCreateSeveralOrders();

		Order GetLatestCompleteOrderForCounterparty(IUnitOfWork UoW, Counterparty counterparty);

		/// <summary>
		/// Список последних заказов для точки доставки.
		/// </summary>
		/// <returns>Список последних заказов для точки доставки.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="deliveryPoint">Точка доставки.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		IList<Order> GetLatestOrdersForDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int? count = null);

		/// <summary>
		/// Список последних заказов для контрагента .
		/// </summary>
		/// <returns>Список последних заказов для контрагента.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="client">Контрагент.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		IList<Order> GetLatestOrdersForCounterparty(IUnitOfWork UoW, Counterparty client, int? count = null);

		/// <summary>
		/// Проверка возможности изменения даты контракта при изменении даты доставки заказа.
		/// Если дата первого заказа меньше newDeliveryDate и это - текущий изменяемый заказ - возвращает True.
		/// Если первый заказ меньше newDeliveryDate и он не является текущим заказом - возвращает False.
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="client">Поиск заказов по этому контрагенту</param>
		/// <param name="newDeliveryDate">Новая дата доставки заказа</param>
		/// <param name="orderId">Текущий изменяемый заказ</param>
		/// <returns>Возможность смены даты контракта</returns>
		bool CanChangeContractDate(IUnitOfWork uow, Counterparty client, DateTime newDeliveryDate, int orderId);

		OrderStatus[] GetOnClosingOrderStatuses();

		Order GetOrderOnDateAndDeliveryPoint(IUnitOfWork uow, DateTime date, DeliveryPoint deliveryPoint);
		IList<Order> GetSameOrderForDateAndDeliveryPoint(IUnitOfWorkFactory uow, DateTime date, DeliveryPoint deliveryPoint);
		Order GetOrder(IUnitOfWork unitOfWork, int orderId);
		IList<Order> GetOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate);

		IList<Order> GetOrdersByCode1c(IUnitOfWork uow, string[] codes1c);

		QueryOver<Order> GetOrdersForRLEditingQuery(DateTime date, bool showShipped, Order orderBaseAlias = null, bool excludeTrucks = false);

		IList<Order> GetOrdersToExport1c8(IUnitOfWork uow, IOrderSettings orderSettings, Export1cMode mode, DateTime startDate, DateTime endDate, int? organizationId = null);

		QueryOver<Order> GetSelfDeliveryOrdersForPaymentQuery();

		OrderStatus[] GetStatusesForActualCount(Order order);

		OrderStatus[] GetStatusesForOrderCancelation();

		OrderStatus[] GetValidStatusesToUseActionBottle();

		bool IsBottleStockExists(IUnitOfWork uow, Counterparty counterparty);

		double GetAvgRandeBetwenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? startDate = null, DateTime? endDate = null);

		double GetAvgRangeBetweenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, out int? orderCount, DateTime? startDate = null, DateTime? endDate = null);

		OrderStatus[] GetUndeliveryStatuses();

		SmsPaymentStatus? GetOrderSmsPaymentStatus(IUnitOfWork uow, int orderId);

		decimal GetCounterpartyDebt(IUnitOfWork uow, int counterpartyId);
		decimal GetCounterpartyWaitingForPaymentOrdersDebt(IUnitOfWork uow, int counterpartyId);
		decimal GetCounterpartyClosingDocumentsOrdersDebtAndNotWaitingForPayment(IUnitOfWork uow, int counterpartyId, IDeliveryScheduleSettings deliveryScheduleSettings);
		decimal GetCounterpartyNotWaitingForPaymentAndNotClosingDocumentsOrdersDebt(IUnitOfWork uow, int counterpartyId, IDeliveryScheduleSettings deliveryScheduleSettings);
		bool IsSelfDeliveryOrderWithoutShipment(IUnitOfWork uow, int orderId);
		bool OrderHasSentReceipt(IUnitOfWork uow, int orderId);
		IEnumerable<Order> GetOrders(IUnitOfWork uow, int[] ids);
		bool HasFlyersOnStock(IUnitOfWork uow, IRouteListSettings routeListSettings, int flyerId, int geographicGroup);

		/// <summary>
		/// Проверка на перенос данной позиция в другой заказ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderItem">Позиция заказа</param>
		/// <returns>true - если свойство CopiedFromUndelivery другого заказа содержит значения, равное Id данной позиции</returns>
		bool IsMovedToTheNewOrder(IUnitOfWork uow, OrderItem orderItem);

		int? GetMaxOrderDailyNumberForDate(IUnitOfWorkFactory uowFactory, DateTime deliveryDate);
		DateTime? GetOrderDeliveryDate(IUnitOfWorkFactory uowFactory, int orderId);
		IList<NotFullyPaidOrderNode> GetAllNotFullyPaidOrdersByClientAndOrg(
			IUnitOfWork uow, int counterpartyId, int organizationId, int closingDocumentDeliveryScheduleId);
		PaymentType GetCurrentOrderPaymentTypeInDB(IUnitOfWork uow, int orderId);
		IEnumerable<Order> GetCashlessOrdersForEdoSendUpd(
			IUnitOfWork uow, DateTime startDate, int organizationId, int closingDocumentDeliveryScheduleId);
		IEnumerable<int> GetNewEdoProcessOrders(IUnitOfWork uow, IEnumerable<int> orderIds);
		IList<EdoContainer> GetPreparingToSendEdoContainers(IUnitOfWork uow, DateTime startDate, int organizationId);
		EdoContainer GetEdoContainerByMainDocumentId(IUnitOfWork uow, string mainDocId);
		EdoContainer GetEdoContainerByDocFlowId(IUnitOfWork uow, Guid? docFlowId);
		IList<EdoContainer> GetEdoContainersByOrderId(IUnitOfWork uow, int orderId);
		IEnumerable<Payment> GetOrderPayments(IUnitOfWork uow, int orderId);
		IList<Order> GetOrdersForTrueMark(IUnitOfWork uow, DateTime? startDate, int organizationId);
		IList<Order> GetOrdersWithSendErrorsForTrueMarkApi(IUnitOfWork uow, DateTime? startDate, int organizationId);
		decimal GetIsAccountableInTrueMarkOrderItemsCount(IUnitOfWork uow, int orderId);
		IList<OrderItem> GetIsAccountableInTrueMarkOrderItems(IUnitOfWork uow, int orderId);
		IList<TrueMarkCancellationDto> GetOrdersForCancellationInTrueMark(IUnitOfWork uow, DateTime startDate, int organizationId);
		IList<OrderOnDayNode> GetOrdersOnDay(IUnitOfWork uow, OrderOnDayFilters orderOnDayFilters);
		IList<Order> GetOrdersForEdoSendBills(IUnitOfWork uow, DateTime startDate, int organizationId, int closingDocumentDeliveryScheduleId);
		OrderStatus[] GetStatusesForOrderCancelationWithCancellation();
		IEnumerable<OrderDto> GetCounterpartyOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		OrderStatus[] GetStatusesForEditGoodsInOrderInRouteList();
		OrderStatus[] GetStatusesForFreeBalanceOperations();
		IList<OrderWithAllocation> GetOrdersWithAllocationsOnDayByOrdersIds(IUnitOfWork uow, IEnumerable<int> orderIds);
		IList<OrderWithAllocation> GetOrdersWithAllocationsOnDayByCounterparty(IUnitOfWork uow, int counterpartyId, IEnumerable<int> orderIds);
		int GetReferredCounterpartiesCountByReferPromotion(IUnitOfWork uow, int referrerId);
		int GetAlreadyReceivedBottlesCountByReferPromotion(IUnitOfWork uow, Order order, int referFriendReasonId);
		bool HasSignedUpdDocumentFromEdo(IUnitOfWork uow, int orderId);
		IQueryable<OksDailyReportOrderDiscountDataNode> GetOrdersDiscountsDataForPeriod(IUnitOfWork uow, DateTime startDate, DateTime endDate);
		IEnumerable<Order> GetOrdersForResendBills(IUnitOfWork uow);
	}
}
