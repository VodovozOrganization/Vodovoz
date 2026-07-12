using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Contracts.Dto;
using DateTimeHelpers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Notifications;
using VodovozBusiness.EntityRepositories.Nodes;

namespace BitrixNotificationsSend.Library.Services
{
	/// <summary>
	/// Сервис формирования и отправки уведомлений по плановым заказам клиентов в Битрикс24
	/// </summary>
	public partial class PlannedOrdersNotificationsSendService
	{
		private readonly OrderStatus[] _completedOrderStatuses =
		{
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,
			OrderStatus.Closed
		};

		private readonly OrderStatus[] _canceledOrderStatuses =
		{
			OrderStatus.Canceled,
			OrderStatus.NotDelivered,
			OrderStatus.DeliveryCanceled
		};

		private readonly ILogger<PlannedOrdersNotificationsSendService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;
		private readonly IPlannedOrdersNotificationsSendClient _plannedOrdersNotificationsSendClient;

		public PlannedOrdersNotificationsSendService(
			ILogger<PlannedOrdersNotificationsSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IBottlesRepository bottlesRepository,
			ICounterpartyRepository counterpartyRepository,
			IDeliveryPointRepository deliveryPointRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IBitrixNotificationsSendSettings bitrixNotificationsSendSettings,
			IPlannedOrdersNotificationsSendClient plannedOrdersNotificationsSendClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings ?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
			_plannedOrdersNotificationsSendClient = plannedOrdersNotificationsSendClient ?? throw new ArgumentNullException(nameof(plannedOrdersNotificationsSendClient));
		}

		/// <summary>
		/// Формирование и отправка уведомлений по клиентам, не сделавшим заказ к плановой дате
		/// </summary>
		/// <returns>true - если уведомления успешно отправлены либо отправлять нечего, false - если отправка не удалась</returns>
		public async Task<bool> SendNotifications(CancellationToken cancellationToken)
		{
			var today = DateTime.UtcNow.ToMoscowDateTime().Date;

			_logger.LogInformation(
				"Начало формирования данных по плановым заказам клиентов на {PlannedOrderDate:yyyy.MM.dd}",
				today);

			IEnumerable<PlannedOrderDto> plannedOrders;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(PlannedOrdersNotificationsSendService)))
			{
				plannedOrders = await GetPlannedOrders(uow, today, cancellationToken);
			}

			if(!plannedOrders.Any())
			{
				_logger.LogInformation("Нет данных по плановым заказам клиентов для отправки в Битрикс24");
				return true;
			}

			_logger.LogInformation(
				"Начало отправки уведомлений по плановым заказам в Битрикс24. Количество строк: {PlannedOrdersCount}",
				plannedOrders.Count());

			var sendNotificationResult =
				await _plannedOrdersNotificationsSendClient.SendPlannedOrdersNotification(plannedOrders, cancellationToken);

			if(sendNotificationResult.IsSuccess)
			{
				_logger.LogInformation("Уведомления по плановым заказам успешно отправлены в Битрикс24");
				return true;
			}

			var message = sendNotificationResult.Errors.FirstOrDefault()?.Message;
			_logger.LogError("Ошибка отправки уведомлений по плановым заказам в Битрикс24: {ErrorMessage}", message);

			return false;
		}

		private async Task<IEnumerable<PlannedOrderDto>> GetPlannedOrders(
			IUnitOfWork uow,
			DateTime today,
			CancellationToken cancellationToken)
		{
			var deliveryPointsCandidates = await GetDeliveryPointsCandidates(uow, today, cancellationToken);
			var selfDeliveryCandidates = await GetSelfDeliveryCandidates(uow, today, cancellationToken);

			_logger.LogInformation(
				"Кандидатов на уведомление после всех фильтров: по точкам доставки {DeliveryPointsCandidatesCount}, " +
				"по самовывозу {SelfDeliveryCandidatesCount}",
				deliveryPointsCandidates.Count,
				selfDeliveryCandidates.Count);

			var allCandidates = deliveryPointsCandidates.Concat(selfDeliveryCandidates).ToList();

			if(!allCandidates.Any())
			{
				return Enumerable.Empty<PlannedOrderDto>();
			}

			await FillCandidatesLastOrders(uow, deliveryPointsCandidates, selfDeliveryCandidates, cancellationToken);

			var deliveryPointIds = deliveryPointsCandidates
				.Select(x => x.Aggregate.DeliveryPointId.Value)
				.ToArray();

			var counterpartyIds = allCandidates
				.Select(x => x.Aggregate.CounterpartyId)
				.Distinct()
				.ToArray();

			var counterpartiesData =
				(await _counterpartyRepository.GetCounterpartiesPlannedOrdersData(uow, counterpartyIds, cancellationToken))
				.ToDictionary(x => x.CounterpartyId);

			var counterpartiesEmails =
				(await _counterpartyRepository.GetCounterpartiesEmailsWithPurpose(uow, counterpartyIds, cancellationToken))
				.GroupBy(x => x.CounterpartyId)
				.ToDictionary(g => g.Key, g => g.ToArray());

			var bottlesDebtsByCounterparties =
				await _bottlesRepository.GetBottlesDebtsByCounterparties(uow, counterpartyIds, cancellationToken);

			var bottlesDebtsByDeliveryPoints = deliveryPointIds.Any()
				? await _bottlesRepository.GetBottlesDebtsByDeliveryPoints(uow, deliveryPointIds, cancellationToken)
				: new Dictionary<int, int>();

			var deliveryPointsAddresses = deliveryPointIds.Any()
				? await _deliveryPointRepository.GetDeliveryPointsCompiledAddresses(uow, deliveryPointIds, cancellationToken)
				: new Dictionary<int, string>();

			var legalCounterpartyIds = counterpartiesData.Values
				.Where(x => x.PersonType == PersonType.legal)
				.Select(x => x.CounterpartyId)
				.ToArray();

			var counterpartiesCashlessDebts = legalCounterpartyIds.Any()
				? await _orderRepository.GetCounterpartiesCashlessDebts(uow, legalCounterpartyIds, cancellationToken)
				: new Dictionary<int, decimal>();

			var plannedOrders = new List<PlannedOrderDto>();

			foreach(var candidate in allCandidates)
			{
				var counterpartyId = candidate.Aggregate.CounterpartyId;
				var deliveryPointId = candidate.Aggregate.DeliveryPointId;
				var isSelfDelivery = deliveryPointId == null;

				counterpartiesData.TryGetValue(counterpartyId, out var counterpartyData);

				counterpartiesEmails.TryGetValue(counterpartyId, out var counterpartyEmails);

				bottlesDebtsByCounterparties.TryGetValue(counterpartyId, out var bottlesDebtByCounterparty);

				int? bottlesDebtByAddress = null;

				string deliveryPointAddress = null;

				if(!isSelfDelivery)
				{
					bottlesDebtByAddress =
						bottlesDebtsByDeliveryPoints.TryGetValue(deliveryPointId.Value, out var addressDebt)
						? addressDebt
						: 0;

					deliveryPointsAddresses.TryGetValue(deliveryPointId.Value, out deliveryPointAddress);
				}

				var isLegalCounterparty = counterpartyData?.PersonType == PersonType.legal;

				var plannedOrder = new PlannedOrderDto
				{
					CounterpartyId = counterpartyId,
					DeliveryPointId = deliveryPointId,
					CounterpartyName = counterpartyData?.FullName,
					CounterpartyInn = counterpartyData?.Inn,
					PhoneNumber = candidate.LastOrder?.ContactPhoneNumber,
					EmailAddress = SelectPriorityEmailAddress(counterpartyEmails),
					DeliveryPointAddress = deliveryPointAddress,
					IsSelfDelivery = isSelfDelivery,
					LastOrderDeliveryDate = candidate.Aggregate.MaxDeliveryDate.Value,
					PlannedOrderDate = candidate.PlannedOrderDate,
					LastOrderBottlesCount = GetLastOrderBottlesCount(candidate.LastOrder),
					BottlesDebtByAddress = bottlesDebtByAddress,
					BottlesDebtByCounterparty = bottlesDebtByCounterparty,
					DelayDaysForCounterparty = isLegalCounterparty ? counterpartyData.DelayDaysForBuyers : 0,
					DebtorDebt =
						isLegalCounterparty && counterpartiesCashlessDebts.TryGetValue(counterpartyId, out var cashlessDebt)
						? cashlessDebt
						: 0
				};

				plannedOrders.Add(plannedOrder);
			}

			return plannedOrders;
		}

		private async Task<IList<PlannedOrderCandidate>> GetDeliveryPointsCandidates(
			IUnitOfWork uow,
			DateTime today,
			CancellationToken cancellationToken)
		{
			var aggregatedData = await _orderRepository.GetDeliveryPointsOrdersAggregatedData(
				uow,
				_completedOrderStatuses,
				_deliveryScheduleSettings,
				cancellationToken);

			_logger.LogInformation(
				"Получены агрегированные данные по заказам точек доставки. Количество точек: {DeliveryPointsCount}",
				aggregatedData.Count);

			var candidates = GetCandidatesWithPlannedDate(
				aggregatedData,
				today);

			_logger.LogInformation(
				"Точек доставки с плановой датой заказа {PlannedOrderDate:yyyy.MM.dd}: {CandidatesCount}",
				today,
				candidates.Count);

			if(!candidates.Any())
			{
				return candidates;
			}

			var deliveryPointIdsWithUpcomingOrders =
				await _orderRepository.GetDeliveryPointIdsWithUpcomingOrders(
					uow,
					candidates.Select(x => x.Aggregate.DeliveryPointId.Value),
					today,
					_canceledOrderStatuses,
					cancellationToken);

			return candidates
				.Where(x => !deliveryPointIdsWithUpcomingOrders.Contains(x.Aggregate.DeliveryPointId.Value))
				.ToList();
		}

		private async Task<IList<PlannedOrderCandidate>> GetSelfDeliveryCandidates(
			IUnitOfWork uow,
			DateTime today,
			CancellationToken cancellationToken)
		{
			var aggregatedData = await _orderRepository.GetSelfDeliveryOrdersAggregatedData(
				uow,
				_completedOrderStatuses,
				_deliveryScheduleSettings,
				cancellationToken);

			_logger.LogInformation(
				"Получены агрегированные данные по самовывозным заказам. Количество контрагентов: {CounterpartiesCount}",
				aggregatedData.Count);

			var candidates = GetCandidatesWithPlannedDate(
				aggregatedData,
				today);

			_logger.LogInformation(
				"Контрагентов с плановой датой самовывозного заказа {PlannedOrderDate:yyyy.MM.dd}: {CandidatesCount}",
				today,
				candidates.Count);

			if(!candidates.Any())
			{
				return candidates;
			}

			var counterpartyIdsWithUpcomingOrders =
				await _orderRepository.GetCounterpartyIdsWithUpcomingSelfDeliveryOrders(
					uow,
					candidates.Select(x => x.Aggregate.CounterpartyId),
					today,
					_canceledOrderStatuses,
					cancellationToken);

			return candidates
				.Where(x => !counterpartyIdsWithUpcomingOrders.Contains(x.Aggregate.CounterpartyId))
				.ToList();
		}

		private async Task FillCandidatesLastOrders(
			IUnitOfWork uow,
			IList<PlannedOrderCandidate> deliveryPointsCandidates,
			IList<PlannedOrderCandidate> selfDeliveryCandidates,
			CancellationToken cancellationToken)
		{
			if(deliveryPointsCandidates.Any())
			{
				var lastOrdersData = await _orderRepository.GetDeliveryPointsLastOrdersData(
					uow,
					deliveryPointsCandidates.Select(x => x.Aggregate.DeliveryPointId.Value),
					deliveryPointsCandidates.Select(x => x.Aggregate.MaxDeliveryDate.Value).Distinct(),
					_completedOrderStatuses,
					_deliveryScheduleSettings,
					cancellationToken);

				var lastOrdersByDeliveryPoints = lastOrdersData
					.GroupBy(x => x.DeliveryPointId.Value)
					.ToDictionary(g => g.Key, g => g.ToArray());

				foreach(var candidate in deliveryPointsCandidates)
				{
					if(lastOrdersByDeliveryPoints.TryGetValue(candidate.Aggregate.DeliveryPointId.Value, out var orders))
					{
						candidate.LastOrder = orders
							.Where(x => x.DeliveryDate == candidate.Aggregate.MaxDeliveryDate)
							.OrderByDescending(x => x.OrderId)
							.FirstOrDefault();
					}
				}
			}

			if(selfDeliveryCandidates.Any())
			{
				var lastOrdersData = await _orderRepository.GetSelfDeliveryLastOrdersData(
					uow,
					selfDeliveryCandidates.Select(x => x.Aggregate.CounterpartyId),
					selfDeliveryCandidates.Select(x => x.Aggregate.MaxDeliveryDate.Value).Distinct(),
					_completedOrderStatuses,
					_deliveryScheduleSettings,
					cancellationToken);

				var lastOrdersByCounterparties = lastOrdersData
					.GroupBy(x => x.CounterpartyId)
					.ToDictionary(g => g.Key, g => g.ToArray());

				foreach(var candidate in selfDeliveryCandidates)
				{
					if(lastOrdersByCounterparties.TryGetValue(candidate.Aggregate.CounterpartyId, out var orders))
					{
						candidate.LastOrder = orders
							.Where(x => x.DeliveryDate == candidate.Aggregate.MaxDeliveryDate)
							.OrderByDescending(x => x.OrderId)
							.FirstOrDefault();
					}
				}
			}
		}

		private IList<PlannedOrderCandidate> GetCandidatesWithPlannedDate(
			IEnumerable<PlannedOrdersAggregatedNode> aggregatedData,
			DateTime plannedOrderDate)
		{
			var daysToNextOrderAfterSingleOrder =
				_bitrixNotificationsSendSettings.PlannedOrdersDaysToNextOrderAfterSingleOrder;

			var candidates = new List<PlannedOrderCandidate>();

			foreach(var node in aggregatedData)
			{
				var calculatedPlannedDate = CalculatePlannedOrderDate(node, daysToNextOrderAfterSingleOrder);

				if(calculatedPlannedDate == plannedOrderDate)
				{
					candidates.Add(new PlannedOrderCandidate
					{
						Aggregate = node,
						PlannedOrderDate = calculatedPlannedDate.Value
					});
				}
			}

			return candidates;
		}

		/// <summary>
		/// Расчет даты планируемого заказа.
		/// Средний интервал между заказами вычисляется как разность дат последнего и первого заказов,
		/// деленная на количество интервалов, что равно среднему значению интервалов между последовательными заказами
		/// </summary>
		private static DateTime? CalculatePlannedOrderDate(PlannedOrdersAggregatedNode node, int daysToNextOrderAfterSingleOrder)
		{
			if(node.MaxDeliveryDate == null || node.OrdersCount < 1)
			{
				return null;
			}

			var lastDeliveryDate = node.MaxDeliveryDate.Value.Date;

			if(node.OrdersCount == 1)
			{
				return lastDeliveryDate.AddDays(daysToNextOrderAfterSingleOrder);
			}

			var averageDaysBetweenOrders =
				(lastDeliveryDate - node.MinDeliveryDate.Value.Date).TotalDays / (node.OrdersCount - 1);

			var orderFrequencyDays = (int)Math.Round(averageDaysBetweenOrders, MidpointRounding.AwayFromZero);

			if(orderFrequencyDays < 1)
			{
				orderFrequencyDays = 1;
			}

			return lastDeliveryDate.AddDays(orderFrequencyDays);
		}

		private static string SelectPriorityEmailAddress(IList<CounterpartyEmailWithPurposeNode> counterpartyEmails)
		{
			if(counterpartyEmails == null || !counterpartyEmails.Any())
			{
				return null;
			}

			var email = counterpartyEmails.FirstOrDefault(e => e.EmailPurpose == EmailPurpose.ForBills)
				?? counterpartyEmails.FirstOrDefault(e => e.EmailPurpose == EmailPurpose.Work)
				?? counterpartyEmails.FirstOrDefault(e => e.EmailPurpose == EmailPurpose.Personal)
				?? counterpartyEmails.FirstOrDefault(e => e.EmailPurpose == EmailPurpose.ForReceipts)
				?? counterpartyEmails.FirstOrDefault();

			return email?.Address;
		}

		private static int GetLastOrderBottlesCount(PlannedOrderLastOrderNode lastOrder)
		{
			if(lastOrder == null)
			{
				return 0;
			}

			return lastOrder.BottlesMovementDelivered ?? (int)lastOrder.WaterBottlesCount;
		}
	}
}
