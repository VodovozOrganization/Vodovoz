using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Contracts.Dto;
using BitrixNotificationsSend.Library.Services.Batches;
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
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
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
	/// Сервис формирования и отправки сделок по плановым заказам клиентов в Битрикс24
	/// </summary>
	public partial class PlannedOrdersDealsCreateService
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

		private readonly ILogger<PlannedOrdersDealsCreateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;
		private readonly IBitrixDealsClient _bitrixDealsClient;
		private readonly IBitrixBatchesSendService _bitrixBatchesSendService;
		private readonly IGenericRepository<PlannedOrder> _plannedOrderRepository;

		public PlannedOrdersDealsCreateService(
			ILogger<PlannedOrdersDealsCreateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IBottlesRepository bottlesRepository,
			ICounterpartyRepository counterpartyRepository,
			IDeliveryPointRepository deliveryPointRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IBitrixNotificationsSendSettings bitrixNotificationsSendSettings,
			IBitrixDealsClient bitrixDealsClient,
			IBitrixBatchesSendService bitrixBatchesSendService,
			IGenericRepository<PlannedOrder> plannedOrderRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings ?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
			_bitrixDealsClient = bitrixDealsClient ?? throw new ArgumentNullException(nameof(bitrixDealsClient));
			_bitrixBatchesSendService = bitrixBatchesSendService ?? throw new ArgumentNullException(nameof(bitrixBatchesSendService));
			_plannedOrderRepository = plannedOrderRepository ?? throw new ArgumentNullException(nameof(plannedOrderRepository));
		}

		/// <summary>
		/// Сбор и сохранение в базу данных данных по клиентам, не сделавшим заказ к плановой дате
		/// Данные сохраняются в стадии "Сделка не создана".
		/// Если данные на текущую дату уже есть в базе, повторный сбор не выполняется
		/// </summary>
		public async Task CollectPlannedOrders(CancellationToken cancellationToken)
		{
			var today = DateTime.UtcNow.ToMoscowDateTime().Date;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Сервис {nameof(PlannedOrdersDealsCreateService)}. Поиск плановых заказов"))
			{
				var hasPlannedOrdersForToday = _plannedOrderRepository
					.Get(uow, x => x.PlannedOrderDate == today, 1)
					.Any();

				if(hasPlannedOrdersForToday)
				{
					_logger.LogInformation(
						"Данные по плановым заказам клиентов на {PlannedOrderDate:yyyy.MM.dd} уже собраны, повторный сбор не выполняется",
						today);

					return;
				}

				_logger.LogInformation(
					"Начало формирования данных по плановым заказам клиентов на {PlannedOrderDate:yyyy.MM.dd}",
					today);

				var plannedOrders = (await GetPlannedOrders(uow, today, cancellationToken)).ToList();

				if(!plannedOrders.Any())
				{
					_logger.LogInformation(
						"Нет данных по плановым заказам клиентов на {PlannedOrderDate:yyyy.MM.dd}",
						today);

					return;
				}

				foreach(var plannedOrder in plannedOrders)
				{
					await uow.SaveAsync(plannedOrder, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Сохранено {PlannedOrdersCount} строк данных по плановым заказам клиентов на {PlannedOrderDate:yyyy.MM.dd}",
					plannedOrders.Count,
					today);
			}
		}

		/// <summary>
		/// Отправка в Битрикс24 запросов на создание сделок по сохранённым на текущую дату планируемым заказам
		/// в стадии "Сделка не создана". После успешного создания сделки стадия меняется на "Сделка создана"
		/// </summary>
		public async Task SendNotCreatedDeals(CancellationToken cancellationToken)
		{
			var today = DateTime.UtcNow.ToMoscowDateTime().Date;

			List<PlannedOrderDto> plannedOrderDtos;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Сервис {nameof(PlannedOrdersDealsCreateService)}. Поиск не созданных сделок"))
			{
				plannedOrderDtos = _plannedOrderRepository
					.Get(uow, x => x.PlannedOrderDate == today && x.Stage == PlannedOrderStage.DealNotCreated)
					.Select(CreatePlannedOrderDto)
					.ToList();
			}

			if(!plannedOrderDtos.Any())
			{
				_logger.LogInformation(
					"Нет плановых заказов клиентов на {PlannedOrderDate:yyyy.MM.dd} в стадии \"Сделка не создана\" для отправки в Битрикс24",
					today);

				return;
			}

			_logger.LogInformation(
				"Начало создания сделок по плановым заказам в Битрикс24. Количество строк: {PlannedOrdersCount}",
				plannedOrderDtos.Count);

			var sendResult = await _bitrixBatchesSendService.SendAll(
				plannedOrderDtos,
				plannedOrderDto => plannedOrderDto.DealCommandKey,
				(batchPlannedOrders, batchCancellationToken) =>
					_bitrixDealsClient.SendPlannedOrderDeals(batchPlannedOrders, batchCancellationToken),
				MarkDealsCreated,
				cancellationToken);

			_logger.LogInformation("Успешно создано {SuccessfulDealsCount} сделок из запланированных {PlannedDealsCount}",
				sendResult.SuccessfulCount,
				plannedOrderDtos.Count);
		}

		private async Task MarkDealsCreated(
			IReadOnlyList<PlannedOrderDto> succeededPlannedOrders,
			CancellationToken cancellationToken)
		{
			var plannedOrderIds = succeededPlannedOrders
				.Select(x => x.PlannedOrderId)
				.ToArray();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Сервис {nameof(PlannedOrdersDealsCreateService)}. Обновление статуса сделок"))
			{
				var createdDealPlannedOrders = _plannedOrderRepository
					.Get(uow, x => plannedOrderIds.Contains(x.Id));

				foreach(var createdDealPlannedOrder in createdDealPlannedOrders)
				{
					createdDealPlannedOrder.Stage = PlannedOrderStage.DealCreated;
					await uow.SaveAsync(createdDealPlannedOrder, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private async Task<IEnumerable<PlannedOrder>> GetPlannedOrders(
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
				return Enumerable.Empty<PlannedOrder>();
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
				(await _counterpartyRepository.GetCounterpartiesPlannedOrdersDataAsync(uow, counterpartyIds, cancellationToken))
				.ToDictionary(x => x.CounterpartyId);

			var counterpartiesEmails =
				(await _counterpartyRepository.GetCounterpartiesEmailsWithPurposeAsync(uow, counterpartyIds, cancellationToken))
				.GroupBy(x => x.CounterpartyId)
				.ToDictionary(g => g.Key, g => g.ToArray());

			var bottlesDebtsByCounterparties =
				await _bottlesRepository.GetBottlesDebtsByCounterpartiesAsync(uow, counterpartyIds, cancellationToken);

			var bottlesDebtsByDeliveryPoints = deliveryPointIds.Any()
				? await _bottlesRepository.GetBottlesDebtsByDeliveryPointsAsync(uow, deliveryPointIds, cancellationToken)
				: new Dictionary<int, int>();

			var deliveryPointsAddresses = deliveryPointIds.Any()
				? await _deliveryPointRepository.GetDeliveryPointsCompiledAddressesAsync(uow, deliveryPointIds, cancellationToken)
				: new Dictionary<int, string>();

			var legalCounterpartyIds = counterpartiesData.Values
				.Where(x => x.PersonType == PersonType.legal)
				.Select(x => x.CounterpartyId)
				.ToArray();

			var counterpartiesCashlessDebts = legalCounterpartyIds.Any()
				? await _orderRepository.GetCounterpartiesCashlessDebtsAsync(uow, legalCounterpartyIds, cancellationToken)
				: new Dictionary<int, decimal>();

			var creationDate = DateTime.UtcNow.ToMoscowDateTime();

			var plannedOrders = new List<PlannedOrder>();

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

				var plannedOrder = new PlannedOrder
				{
					CreationDate = creationDate,
					Stage = PlannedOrderStage.DealNotCreated,
					CounterpartyId = counterpartyId,
					DeliveryPointId = deliveryPointId,
					CounterpartyName = counterpartyData?.FullName,
					CounterpartyInn = counterpartyData?.Inn,
					PhoneNumber = candidate.LastOrder?.ContactPhoneNumber,
					EmailAddress = SelectPriorityEmailAddress(counterpartyEmails),
					DeliveryPointAddress = deliveryPointAddress?.Substring(0, Math.Min(deliveryPointAddress.Length, 1000)),
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
			var aggregatedData = await _orderRepository.GetDeliveryPointsOrdersAggregatedDataAsync(
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
				await _orderRepository.GetDeliveryPointIdsWithUpcomingOrdersAsync(
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
			var aggregatedData = await _orderRepository.GetSelfDeliveryOrdersAggregatedDataAsync(
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
				await _orderRepository.GetCounterpartyIdsWithUpcomingSelfDeliveryOrdersAsync(
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
				var lastOrdersData = await _orderRepository.GetDeliveryPointsLastOrdersDataAsync(
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
				var lastOrdersData = await _orderRepository.GetSelfDeliveryLastOrdersDataAsync(
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

		private static PlannedOrderDto CreatePlannedOrderDto(PlannedOrder plannedOrder) =>
			new PlannedOrderDto
			{
				PlannedOrderId = plannedOrder.Id,
				CounterpartyId = plannedOrder.CounterpartyId,
				DeliveryPointId = plannedOrder.DeliveryPointId,
				IsSelfDelivery = plannedOrder.IsSelfDelivery,
				CounterpartyName = plannedOrder.CounterpartyName,
				CounterpartyInn = plannedOrder.CounterpartyInn,
				PhoneNumber = plannedOrder.PhoneNumber,
				EmailAddress = plannedOrder.EmailAddress,
				DeliveryPointAddress = plannedOrder.DeliveryPointAddress,
				LastOrderDeliveryDate = plannedOrder.LastOrderDeliveryDate,
				PlannedOrderDate = plannedOrder.PlannedOrderDate,
				LastOrderBottlesCount = plannedOrder.LastOrderBottlesCount,
				BottlesDebtByAddress = plannedOrder.BottlesDebtByAddress,
				BottlesDebtByCounterparty = plannedOrder.BottlesDebtByCounterparty,
				DelayDaysForCounterparty = plannedOrder.DelayDaysForCounterparty,
				DebtorDebt = plannedOrder.DebtorDebt
			};
	}
}
