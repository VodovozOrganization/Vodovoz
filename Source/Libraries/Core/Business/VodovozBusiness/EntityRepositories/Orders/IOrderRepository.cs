using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Orders.Default;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Payments;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Orders;
using VodovozBusiness.EntityRepositories.Nodes;
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

		Order GetOrderOnDateAndDeliveryPoint(IUnitOfWork uow, DateTime date, DeliveryPoint deliveryPoint);
		IList<Order> GetSameOrderForDateAndDeliveryPoint(IUnitOfWorkFactory uow, DateTime date, DeliveryPoint deliveryPoint);
		Order GetOrder(IUnitOfWork unitOfWork, int orderId);

		/// <summary>
		/// Получить заказ по его идентификатору
		/// </summary>
		/// <param name="unitOfWork">IUnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Объект заказа</returns>
		Task<Order> GetOrderByIdAsync(IUnitOfWork unitOfWork, int orderId, CancellationToken cancellationToken);

		IList<Order> GetOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate);

		IList<Order> GetOrdersByCode1c(IUnitOfWork uow, string[] codes1c);

		QueryOver<Order> GetOrdersForRLEditingQuery(DateTime date, bool showShipped, Order orderBaseAlias = null, bool excludeTrucks = false);

		IList<Order> GetOrdersToExport1c8(IUnitOfWork uow, IOrderSettings orderSettings, Export1cMode exportMode, DateTime startDate, DateTime endDate, int? organizationId = null);

		QueryOver<Order> GetSelfDeliveryOrdersForPaymentQuery();

		OrderStatus[] GetStatusesForActualCount(Order order);

		OrderStatus[] GetStatusesForOrderCancelation();

		OrderStatus[] GetValidStatusesToUseActionBottle();

		bool IsBottleStockExists(IUnitOfWork uow, Counterparty counterparty);

		double GetAvgRandeBetwenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? startDate = null, DateTime? endDate = null);

		double GetAvgRangeBetweenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, out int? orderCount, DateTime? startDate = null, DateTime? endDate = null);

		OrderStatus[] GetUndeliveryStatuses();

		/// <summary>
		/// Статусы заказов для отмены и переноса онлайн-заказов, при которых разрешается выполнять эти действия
		/// </summary>
		/// <returns>Массив статусов</returns>
		OrderStatus[] GetStatusesForTransferOrCancellationOnlineOrder();

		SmsPaymentStatus? GetOrderSmsPaymentStatus(IUnitOfWork uow, int orderId);

		decimal GetCounterpartyDebt(IUnitOfWork uow, int counterpartyId, int? organizationId = null);
		decimal GetCounterpartyWaitingForPaymentOrdersDebt(IUnitOfWork uow, int counterpartyId, int? organizationId = null);
		decimal GetCounterpartyClosingDocumentsOrdersDebtAndNotWaitingForPayment(
			IUnitOfWork uow, int counterpartyId, IDeliveryScheduleSettings deliveryScheduleSettings, int? organizationId = null);
		decimal GetCounterpartyNotWaitingForPaymentAndNotClosingDocumentsOrdersDebt(
			IUnitOfWork uow, int counterpartyId, IDeliveryScheduleSettings deliveryScheduleSettings, int? organizationId = null);
		bool IsSelfDeliveryOrderWithoutShipment(IUnitOfWork uow, int orderId);
		bool OrderHasSentReceipt(IUnitOfWork uow, int orderId);
		bool OrderHasSentUPD(IUnitOfWork uow, int orderId);
		IEnumerable<Order> GetOrders(IUnitOfWork uow, int[] ids);
		/// <summary>
		/// Получение списка идентификаторов неоплаченных безналичных заказов контрагента за указанный период
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="counterpartyId"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="organizationId"></param>
		/// <returns></returns>
		IList<int> GetUnpaidOrdersIds(IUnitOfWork uow, int counterpartyId, DateTime? startDate, DateTime? endDate, int organizationId);
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
			IUnitOfWork uow, DateTime startDate, int closingDocumentDeliveryScheduleId);
		IEnumerable<int> GetNewEdoProcessOrders(IUnitOfWork uow, IEnumerable<int> orderIds);
		IList<EdoContainer> GetPreparingToSendEdoContainers(IUnitOfWork uow, DateTime startDate, int organizationId);
		EdoContainer GetEdoContainerByMainDocumentId(IUnitOfWork uow, string mainDocId);
		EdoContainer GetEdoContainerByDocFlowId(IUnitOfWork uow, Guid? docFlowId);
		IList<EdoContainer> GetEdoContainersByOrderId(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получение заказа по идентификатору исходящего ЭДО документа
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="orderEdoDocumentId"></param>
		/// <returns></returns>
		OrderEntity GetOrderByOrderEdoDocumentId(IUnitOfWork uow, int orderEdoDocumentId);

		IEnumerable<Payment> GetOrderPayments(IUnitOfWork uow, int orderId);
		/// <summary>
		/// Получение списка связанных строк заказа
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="orderId"></param>
		/// <returns></returns>
		IList<OrderItem> GetOrderItems(IUnitOfWork uow, int orderId);
		IList<Order> GetOrdersForTrueMark(IUnitOfWork uow, DateTime? startDate, int organizationId);
		IList<Order> GetOrdersWithSendErrorsForTrueMarkApi(IUnitOfWork uow, DateTime? startDate, int organizationId);
		decimal GetIsAccountableInTrueMarkOrderItemsCount(IUnitOfWork uow, int orderId);
		IList<OrderItem> GetIsAccountableInTrueMarkOrderItems(IUnitOfWork uow, int orderId);
		IList<TrueMarkCancellationDto> GetOrdersForCancellationInTrueMark(IUnitOfWork uow, DateTime startDate, int organizationId);
		IList<OrderOnDayNode> GetOrdersOnDay(IUnitOfWork uow, OrderOnDayFilters orderOnDayFilters);
		IList<Order> GetOrdersForEdoSendBills(IUnitOfWork uow, DateTime startDate, int closingDocumentDeliveryScheduleId);
		OrderStatus[] GetStatusesForOrderCancelationWithCancellation();
		IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		IEnumerable<Vodovoz.Core.Data.Orders.V4.OrderDto> GetCounterpartyOrdersFromOnlineOrdersV4(
			IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		IEnumerable<Vodovoz.Core.Data.Orders.V4.OrderDto> GetCounterpartyOrdersWithoutOnlineOrdersV4(
			IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);

		/// <summary>
		/// Получение заказов контрагента, которые связаны с онлайн-заказами
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, с которой доступна рейтинговая информация</param>
		/// <param name="orderStatuses">Статусы заказов</param>
		/// <returns>Список заказов</returns>
		IEnumerable<Core.Data.Orders.V6.OrderDto> GetCounterpartyOrdersFromOnlineOrdersV6(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom, IEnumerable<ExternalOrderStatus> orderStatuses = null);

		/// <summary>
		/// Получение заказов контрагента, которые не связаны с онлайн-заказами
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="ratingAvailableFrom">Дата, с которой доступна рейтинговая информация</param>
		/// <param name="orderStatuses">Статусы заказов</param>
		/// <returns>Список заказов</returns>
		IEnumerable<Core.Data.Orders.V6.OrderDto> GetCounterpartyOrdersWithoutOnlineOrdersV6(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom, IEnumerable<ExternalOrderStatus> orderStatuses = null);

		IEnumerable<Order> GetOrdersFromOnlineOrder(IUnitOfWork uow, int onlineOrderId);
		OrderStatus[] GetStatusesForEditGoodsInOrderInRouteList();
		OrderStatus[] GetStatusesForFreeBalanceOperations();
		IList<OrderWithAllocation> GetOrdersWithAllocationsOnDayByOrdersIds(IUnitOfWork uow, IEnumerable<int> orderIds, int organizationId);
		IList<OrderWithAllocation> GetOrdersWithAllocationsOnDayByCounterparty(IUnitOfWork uow, int counterpartyId, IEnumerable<int> orderIds, int organizationId);
		IList<OrderWithAllocation> GetAllocationsToOrdersWithAnotherClient(
		IUnitOfWork uow, int counterpartyId, string counterpartyInn, IEnumerable<int> exceptOrderIds, int organizationId);
		int GetReferredCounterpartiesCountByReferPromotion(IUnitOfWork uow, int referrerId);
		int GetAlreadyReceivedBottlesCountByReferPromotion(IUnitOfWork uow, Order order, int referFriendReasonId);
		bool HasSignedUpdDocumentFromEdo(IUnitOfWork uow, int orderId);
		IQueryable<OksDailyReportOrderDiscountDataNode> GetOrdersDiscountsDataForPeriod(IUnitOfWork uow, DateTime startDate, DateTime endDate);
		IEnumerable<Order> GetOrdersForResendBills(IUnitOfWork uow, int? organizationId = null);

		/// <summary>
		/// Получить все добавленные коды ЧЗ для указанного заказа с доставкой
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Список кодов TrueMark</returns>
		IList<RouteListItemTrueMarkProductCode> GetAddedRouteListItemTrueMarkProductCodesByOrderId(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Проверяет, все ли коды ЧЗ добавлены к указанному заказу с доставкой
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>True, если все коды ЧЗ добавлены, иначе False</returns>
		bool IsAllRouteListItemTrueMarkProductCodesAddedToOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получить все добавленные водителем коды ЧЗ для указанной строки заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderItemId">Номер строки заказа</param>
		/// <returns>Список с данными номера кода и номера строки заказа</returns>
		IList<TrueMarkProductCodeOrderItem> GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(IUnitOfWork uow, int orderItemId);

		/// <summary>
		/// Получить все добавленные водителем коды ЧЗ для указанного заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Список с данными номера кода и номера строки заказа</returns>
		IList<TrueMarkProductCodeOrderItem> GetTrueMarkCodesAddedByDriverToOrderByOrderId(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получить все коды ЧЗ добавленные к заказу на складе
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Список кодов</returns>
		IList<TrueMarkWaterIdentificationCode> GetTrueMarkCodesAddedInWarehouseToOrderByOrderId(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Проверяет, находится ли документ погрузки для указанного заказа в статусе Погрузка завершена
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>True, если погрузка завершена, иначе False</returns>
		bool IsOrderCarLoadDocumentLoadOperationStateDone(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Проверяет, что все коды, отсканированные водителем для указанного заказа были обработаны
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="cancellationToken"></param>
		/// <returns>True, если все коды были обработаны, иначе False</returns>
		Task<bool> IsAllDriversScannedCodesInOrderProcessed(IUnitOfWork uow, int orderId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Возвращает агрегированные данные по неоплаченным безналичным заказам для указанной организации, сгруппированные по контрагенту
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="organizationId">Id организации</param>
		/// <param name="orderStatuses">Статусы заказов</param>
		/// <param name="counterpartyTypes">Типы контагентов</param>
		/// <param name="excludeCounterpartyRevenueStatuses">Статусы контрагентов в налоговой для исключения</param>
		/// <param name="excludeCloseDeliveryDebtTypes">Тип задолженности контрагента для исключения</param>
		/// <param name="tenderCameFromId">Id источника клиента Тендер</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные по неоплаченным заказам</returns>
		Task<IDictionary<int, CounterpartyOrdersAggregatedNode>> GetNotPaidCashlessOrdersData(
			IUnitOfWork uow,
			int organizationId,
			IEnumerable<OrderStatus> orderStatuses,
			IEnumerable<CounterpartyType> counterpartyTypes,
			IEnumerable<RevenueStatus> excludeCounterpartyRevenueStatuses,
			IEnumerable<DebtType> excludeCloseDeliveryDebtTypes,
			int tenderCameFromId,
			CancellationToken cancellationToken);
		
		/// <summary>
		/// Получение идентификаторов заказов на дату по клиенту и ТД
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="date">Дата доставки заказа</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <param name="deliveryPointId">Идентификатор ТД</param>
		/// <returns></returns>
		IEnumerable<int> GetClientOrdersIdsForDate(IUnitOfWork uow, DateTime date, int? counterpartyId, int? deliveryPointId);

		/// <summary>
		/// Получение просроченных задолженностей по контрагентам c открытыми поставками
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="daysBeforeClosingDeliveries">Количество дней до закрытия поставок</param>
		/// <param name="organizationsIds">Id организаций</param>
		/// <param name="orderStatuses">Статусы заказов</param>
		/// <param name="counterpartyTypes">Типы контрагентов</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="debtThreshold">Задолженность свыше</param>
		/// <returns>Данные по задолженности</returns>
		Task<IReadOnlyCollection<OverdueDebtOverPeriodLimitAggregateNode>> GetWithoutClosedDeliveriesCounterpartiesOverdueDebts(
			IUnitOfWork unitOfWork, 
			int daysBeforeClosingDeliveries, 
			int[] organizationsIds, 
			OrderStatus[] orderStatuses, 
			CounterpartyType[] counterpartyTypes,
			int tenderCameFromId,
			decimal debtThreshold,
			int? counterpartyId = null,			
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Проверяет наличие просроченной задолженности контрагента c закрытыми поставками
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="daysBeforeClosingDeliveries">Количество дней до закрытия поставок</param>
		/// <param name="organizationsIds">Id организаций</param>
		/// <param name="orderStatuses">Статусы заказов</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <returns>Данные по задолженности</returns>
		Task<bool> HasClosedDeliveriesCounterpartyWithOverdueDebtsAsync(
			IUnitOfWork unitOfWork,
			int daysBeforeClosingDeliveries, 
			int[] organizationsIds,
			OrderStatus[] orderStatuses,
			int counterpartyId,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Получение данных по просроченной дебиторской задолженности контрагентов для формирования претензионных писем, 
		/// сгруппированные по контрагенту и организации, с учетом минимального количества дней просрочки сверх установленного для КА
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="expiredMinDaysAgo">Минимальное количество дней просрочки</param>
		/// <param name="excludeCounterpartyRevenueStatuses">Статусы контрагентов в налоговой для исключения</param>
		/// <param name="letterOfClaimResendIntervalDays">Интервал дней для повторной отправки претензии</param>
		/// <param name="maxClientsToTake">Максимальное количество клиентов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные по просроченным долгам контрагента в разрезе организации</returns>
		Task<IEnumerable<CounterpartyOverdueDebtorDebtAggregatedNode>> GetOverdueDebtorDebtDataForLettersOfClaim(
			IUnitOfWork uow,
			int expiredMinDaysAgo,
			IEnumerable<RevenueStatus> excludeCounterpartyRevenueStatuses,
			int letterOfClaimResendIntervalDays,
			int maxClientsToTake = int.MaxValue,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Получить первый доставленный заказ из массива идентификаторов заказов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderIds">Массив идентификаторов заказов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Наиболее ранний заказ</returns>
		Task<Order> GetEarliestOrder(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получение добавочного номера Mango водителя, доставляющего заказ с указанным номером
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Добавочный номер Mango водителя</returns>
		Task<DriverMangoExtensionNumber> GetDriversMangoExtensionNumberByOrderId(IUnitOfWork uow, int orderId, CancellationToken cancellationToken);

		/// <summary>
		/// Получение идентификаторов заказов контрагента,
		/// созданных начиная с указанной даты и не находящихся в исключаемых статусах
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="startDate">Дата, начиная с которой ищутся заказы (по дате создания заказа)</param>
		/// <param name="excludedOrderStatuses">Статусы заказов, исключаемые из выборки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификаторы найденных заказов</returns>
		Task<IEnumerable<int>> GetOrderIdsByCounterpartyFromDate(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime startDate,
			IEnumerable<OrderStatus> excludedOrderStatuses,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получение идентификаторов заказов контрагента на указанные точки доставки,
		/// созданных начиная с указанной даты и не находящихся в исключаемых статусах
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="deliveryPointIds">Идентификаторы точек доставки</param>
		/// <param name="startDate">Дата, начиная с которой ищутся заказы (по дате создания заказа)</param>
		/// <param name="excludedOrderStatuses">Статусы заказов, исключаемые из выборки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификаторы найденных заказов</returns>
		Task<IEnumerable<int>> GetOrderIdsByCounterpartyAndDeliveryPointsFromDate(
			IUnitOfWork uow,
			int counterpartyId,
			IEnumerable<int> deliveryPointIds,
			DateTime startDate,
			IEnumerable<OrderStatus> excludedOrderStatuses,
			CancellationToken cancellationToken);
	}
}
