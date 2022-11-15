using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Services;

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
		int Get19LWatterQtyForOrder(IUnitOfWork uow, Domain.Orders.Order order);

		IList<Domain.Orders.Order> GetAcceptedOrdersForRegion(IUnitOfWork uow, DateTime date, District district);

		/// <summary>
		/// Список МЛ для заказа, отсортированный в порядке владения этим заказом, в случае переносов
		/// </summary>
		/// <returns>Список МЛ</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="order">Заказ</param>
		IList<RouteList> GetAllRLForOrder(IUnitOfWork UoW, Domain.Orders.Order order);

		Dictionary<int, IEnumerable<int>> GetAllRouteListsForOrders(IUnitOfWork UoW, IEnumerable<Domain.Orders.Order> orders);

		IList<Domain.Orders.Order> GetCurrentOrders(IUnitOfWork UoW, Counterparty counterparty);

		/// <summary>
		/// Оборудование заказа от клиента
		/// </summary>
		/// <returns>Список оборудования от клиенту</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		IList<ClientEquipmentNode> GetEquipmentFromClientForOrder(IUnitOfWork uow, Domain.Orders.Order order);

		IList<ClientEquipmentNode> GetEquipmentToClientForOrder(IUnitOfWork uow, Domain.Orders.Order order);

		/// <summary>
		/// Первый заказ контрагента, который можно считать выполненым.
		/// </summary>
		/// <returns>Первый заказ</returns>
		/// <param name="uow">UoW</param>
		/// <param name="client">Контрагент</param>
		Domain.Orders.Order GetFirstRealOrderForClientForActionBottle(IUnitOfWork uow, Domain.Orders.Order order, Counterparty client);

		OrderStatus[] GetGrantedStatusesToCreateSeveralOrders();

		Domain.Orders.Order GetLatestCompleteOrderForCounterparty(IUnitOfWork UoW, Counterparty counterparty);

		/// <summary>
		/// Список последних заказов для точки доставки.
		/// </summary>
		/// <returns>Список последних заказов для точки доставки.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="deliveryPoint">Точка доставки.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		IList<Domain.Orders.Order> GetLatestOrdersForDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int? count = null);

		/// <summary>
		/// Список последних заказов для контрагента .
		/// </summary>
		/// <returns>Список последних заказов для контрагента.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="client">Контрагент.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		IList<Domain.Orders.Order> GetLatestOrdersForCounterparty(IUnitOfWork UoW, Counterparty client, int? count = null);

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

		Domain.Orders.Order GetOrderOnDateAndDeliveryPoint(IUnitOfWork uow, DateTime date, DeliveryPoint deliveryPoint);
		IList<Domain.Orders.Order> GetSameOrderForDateAndDeliveryPoint(IUnitOfWorkFactory uow, DateTime date, DeliveryPoint deliveryPoint);
        Domain.Orders.Order GetOrder(IUnitOfWork unitOfWork, int orderId);
        IList<Domain.Orders.Order> GetOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate);

		IList<Domain.Orders.Order> GetOrdersByCode1c(IUnitOfWork uow, string[] codes1c);

		QueryOver<Domain.Orders.Order> GetOrdersForRLEditingQuery(DateTime date, bool showShipped, Domain.Orders.Order orderBaseAlias = null);
		
		IList<Domain.Orders.Order> GetOrdersToExport1c8(IUnitOfWork uow, IOrderParametersProvider orderParametersProvider, Export1cMode mode, DateTime startDate, DateTime endDate, int? organizationId = null);

		QueryOver<Domain.Orders.Order> GetSelfDeliveryOrdersForPaymentQuery();

		OrderStatus[] GetStatusesForActualCount(Domain.Orders.Order order);

		OrderStatus[] GetStatusesForOrderCancelation();

		OrderStatus[] GetValidStatusesToUseActionBottle();

		bool IsBottleStockExists(IUnitOfWork uow, Counterparty counterparty);

		double GetAvgRandeBetwenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? startDate = null, DateTime? endDate = null);

		double GetAvgRangeBetweenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, out int? orderCount, DateTime? startDate = null, DateTime? endDate = null);

		OrderStatus[] GetUndeliveryStatuses();

		/// <summary>
		/// Подбирает подходящие заказы, для которых необходимо отправкить чеки контрагентам
		/// </summary>
		IEnumerable<ReceiptForOrderNode> GetOrdersForCashReceiptServiceToSend(
			IUnitOfWork uow,
			IOrderParametersProvider orderParametersProvider,
			DateTime? startDate = null);

		bool IsOrderCloseWithoutDelivery(IUnitOfWork uow, Domain.Orders.Order order);

		SmsPaymentStatus? GetOrderSmsPaymentStatus(IUnitOfWork uow, int orderId);

		decimal GetCounterpartyDebt(IUnitOfWork uow, int counterpartyId);

		bool IsSelfDeliveryOrderWithoutShipment(IUnitOfWork uow, int orderId);
		bool OrderHasSentReceipt(IUnitOfWork uow, int orderId);
		IEnumerable<Domain.Orders.Order> GetOrders(IUnitOfWork uow, int[] ids);
		bool HasFlyersOnStock(IUnitOfWork uow, IRouteListParametersProvider routeListParametersProvider, int flyerId, int geographicGroup);
		int? GetMaxOrderDailyNumberForDate(IUnitOfWorkFactory uowFactory, DateTime deliveryDate);
		DateTime? GetOrderDeliveryDate(IUnitOfWorkFactory uowFactory, int orderId);
		IList<NotFullyPaidOrderNode> GetAllNotFullyPaidOrdersByClientAndOrg(
			IUnitOfWork uow, int counterpartyId, int organizationId, int closingDocumentDeliveryScheduleId);
		PaymentType GetCurrentOrderPaymentTypeInDB(IUnitOfWork uow, int orderId);
	}

	public class ClientEquipmentNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public int Count { get; set; }
	}
}
