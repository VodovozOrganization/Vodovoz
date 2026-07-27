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
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Library.Services
{
	/// <summary>
	/// Сервис формирования и отправки сделок по последним сервисным заказам клиентов в Битрикс24
	/// </summary>
	public class LastServiceOrdersDealsCreateService
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

		private readonly ILogger<LastServiceOrdersDealsCreateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly IGeneralSettings _generalSettings;
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;
		private readonly IBitrixDealsClient _bitrixDealsClient;
		private readonly IBitrixBatchesSendService _bitrixBatchesSendService;
		private readonly IGenericRepository<LastServiceOrder> _lastServiceOrderRepository;

		public LastServiceOrdersDealsCreateService(
			ILogger<LastServiceOrdersDealsCreateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			ICounterpartyRepository counterpartyRepository,
			IDeliveryPointRepository deliveryPointRepository,
			IGeneralSettings generalSettings,
			IBitrixNotificationsSendSettings bitrixNotificationsSendSettings,
			IBitrixDealsClient bitrixDealsClient,
			IBitrixBatchesSendService bitrixBatchesSendService,
			IGenericRepository<LastServiceOrder> lastServiceOrderRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings ?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
			_bitrixDealsClient = bitrixDealsClient ?? throw new ArgumentNullException(nameof(bitrixDealsClient));
			_bitrixBatchesSendService = bitrixBatchesSendService ?? throw new ArgumentNullException(nameof(bitrixBatchesSendService));
			_lastServiceOrderRepository = lastServiceOrderRepository ?? throw new ArgumentNullException(nameof(lastServiceOrderRepository));
		}

		/// <summary>
		/// Сбор и сохранение в базу данных данных по последним сервисным заказам клиентов.
		/// Данные сохраняются в стадии "Сделка не создана".
		/// Если сбор на текущую дату уже выполнялся, повторный сбор не выполняется
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		public async Task CollectLastServiceOrders(CancellationToken cancellationToken)
		{
			var today = DateTime.UtcNow.ToMoscowDateTime().Date;
			var tomorrow = today.AddDays(1);

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Сервис {nameof(LastServiceOrdersDealsCreateService)}. Поиск последних сервисных заказов"))
			{
				var hasCollectedDataForToday = _lastServiceOrderRepository
					.Get(uow, x => x.CreationDate >= today && x.CreationDate < tomorrow, 1)
					.Any();

				if(hasCollectedDataForToday)
				{
					_logger.LogInformation(
						"Данные по последним сервисным заказам клиентов на {CollectDate:yyyy.MM.dd} уже собраны, повторный сбор не выполняется",
						today);

					return;
				}

				var serviceNomenclatureIds = _generalSettings.ServiceNomenclaturesForBitrixDeals;

				if(serviceNomenclatureIds is null || !serviceNomenclatureIds.Any())
				{
					_logger.LogWarning(
						"Список номенклатур сервисного центра для создания сделок Битрикс24 пуст, сбор данных не выполняется");

					return;
				}

				_logger.LogInformation(
					"Начало формирования данных по последним сервисным заказам клиентов на {CollectDate:yyyy.MM.dd}",
					today);

				var lastServiceOrders = (await GetLastServiceOrders(uow, today, serviceNomenclatureIds, cancellationToken)).ToList();

				if(!lastServiceOrders.Any())
				{
					_logger.LogInformation(
						"Нет данных по последним сервисным заказам клиентов на {CollectDate:yyyy.MM.dd}",
						today);

					return;
				}

				foreach(var lastServiceOrder in lastServiceOrders)
				{
					await uow.SaveAsync(lastServiceOrder, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Сохранено {LastServiceOrdersCount} строк данных по последним сервисным заказам клиентов на {CollectDate:yyyy.MM.dd}",
					lastServiceOrders.Count,
					today);
			}
		}

		/// <summary>
		/// Отправка в Битрикс24 запросов на создание сделок по сохранённым последним сервисным заказам
		/// в стадии "Сделка не создана". После успешного создания сделки стадия меняется на "Сделка создана"
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		public async Task SendNotCreatedDeals(CancellationToken cancellationToken)
		{
			List<LastServiceOrderDto> lastServiceOrderDtos;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Сервис {nameof(LastServiceOrdersDealsCreateService)}. Поиск не созданных сделок"))
			{
				lastServiceOrderDtos = _lastServiceOrderRepository
					.Get(uow, x => x.Stage == BitrixDealCreationStage.DealNotCreated)
					.Select(CreateLastServiceOrderDto)
					.ToList();
			}

			if(!lastServiceOrderDtos.Any())
			{
				_logger.LogInformation(
					"Нет последних сервисных заказов клиентов в стадии \"Сделка не создана\" для отправки в Битрикс24");

				return;
			}

			_logger.LogInformation(
				"Начало создания сделок по последним сервисным заказам в Битрикс24. Количество строк: {LastServiceOrdersCount}",
				lastServiceOrderDtos.Count);

			var sendResult = await _bitrixBatchesSendService.SendAll(
				lastServiceOrderDtos,
				lastServiceOrderDto => lastServiceOrderDto.DealCommandKey,
				(batchLastServiceOrders, batchCancellationToken) =>
					_bitrixDealsClient.LastServiceOrderDeals(batchLastServiceOrders, batchCancellationToken),
				MarkDealsCreated,
				cancellationToken);

			_logger.LogInformation("Успешно создано {SuccessfulDealsCount} сделок из запланированных {PlannedDealsCount}",
				sendResult.SuccessfulCount,
				lastServiceOrderDtos.Count);
		}

		private async Task MarkDealsCreated(
			IReadOnlyList<LastServiceOrderDto> succeededLastServiceOrders,
			CancellationToken cancellationToken)
		{
			var lastServiceOrderIds = succeededLastServiceOrders
				.Select(x => x.LastServiceOrderId)
				.ToArray();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Сервис {nameof(LastServiceOrdersDealsCreateService)}. Обновление статуса сделок"))
			{
				var createdDealLastServiceOrders = _lastServiceOrderRepository
					.Get(uow, x => lastServiceOrderIds.Contains(x.Id));

				foreach(var createdDealLastServiceOrder in createdDealLastServiceOrders)
				{
					createdDealLastServiceOrder.Stage = BitrixDealCreationStage.DealCreated;
					await uow.SaveAsync(createdDealLastServiceOrder, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private async Task<IEnumerable<LastServiceOrder>> GetLastServiceOrders(
			IUnitOfWork uow,
			DateTime today,
			int[] serviceNomenclatureIds,
			CancellationToken cancellationToken)
		{
			var maxLastOrderDeliveryDate =
				today.AddDays(-_bitrixNotificationsSendSettings.LastServiceOrdersMinDaysSinceLastOrder);

			var minLastOrderDeliveryDate = _bitrixNotificationsSendSettings.LastServiceOrdersMinDeliveryDate.Date;

			var candidates = (await _orderRepository.GetCounterpartiesLastServiceOrdersDataAsync(
					uow,
					serviceNomenclatureIds,
					_completedOrderStatuses,
					minLastOrderDeliveryDate,
					maxLastOrderDeliveryDate,
					cancellationToken))
				.ToList();

			_logger.LogInformation(
				"Контрагентов с не обработанным ранее последним сервисным заказом " +
				"не ранее {MinLastOrderDeliveryDate:yyyy.MM.dd} " +
				"и не позднее {MaxLastOrderDeliveryDate:yyyy.MM.dd}: {CandidatesCount}",
				minLastOrderDeliveryDate,
				maxLastOrderDeliveryDate,
				candidates.Count);

			if(!candidates.Any())
			{
				return Enumerable.Empty<LastServiceOrder>();
			}

			var counterpartyIdsWithUpcomingOrders =
				await _orderRepository.GetCounterpartyIdsWithUpcomingServiceOrdersAsync(
					uow,
					candidates.Select(x => x.CounterpartyId),
					today,
					_canceledOrderStatuses,
					serviceNomenclatureIds,
					cancellationToken);

			candidates = candidates
				.Where(x => !counterpartyIdsWithUpcomingOrders.Contains(x.CounterpartyId))
				.ToList();

			_logger.LogInformation(
				"Кандидатов на создание сделок после всех фильтров: {CandidatesCount}",
				candidates.Count);

			if(!candidates.Any())
			{
				return Enumerable.Empty<LastServiceOrder>();
			}

			var counterpartyIds = candidates
				.Select(x => x.CounterpartyId)
				.Distinct()
				.ToArray();

			var counterpartiesData =
				(await _counterpartyRepository.GetCounterpartiesPlannedOrdersDataAsync(uow, counterpartyIds, cancellationToken))
				.ToDictionary(x => x.CounterpartyId);

			var counterpartiesEmails =
				(await _counterpartyRepository.GetCounterpartiesEmailsWithPurposeAsync(uow, counterpartyIds, cancellationToken))
				.GroupBy(x => x.CounterpartyId)
				.ToDictionary(g => g.Key, g => g.ToArray());

			var deliveryPointIds = candidates
				.Where(x => x.DeliveryPointId != null)
				.Select(x => x.DeliveryPointId.Value)
				.Distinct()
				.ToArray();

			var deliveryPointsAddresses = deliveryPointIds.Any()
				? await _deliveryPointRepository.GetDeliveryPointsCompiledAddressesAsync(uow, deliveryPointIds, cancellationToken)
				: new Dictionary<int, string>();

			var creationDate = DateTime.UtcNow.ToMoscowDateTime();

			var lastServiceOrders = new List<LastServiceOrder>();

			foreach(var candidate in candidates)
			{
				counterpartiesData.TryGetValue(candidate.CounterpartyId, out var counterpartyData);

				counterpartiesEmails.TryGetValue(candidate.CounterpartyId, out var counterpartyEmails);

				string deliveryPointAddress = null;

				if(candidate.DeliveryPointId != null)
				{
					deliveryPointsAddresses.TryGetValue(candidate.DeliveryPointId.Value, out deliveryPointAddress);
				}

				var lastServiceOrder = new LastServiceOrder
				{
					CreationDate = creationDate,
					Stage = BitrixDealCreationStage.DealNotCreated,
					CounterpartyId = candidate.CounterpartyId,
					CounterpartyName = counterpartyData?.FullName,
					DeliveryPointId = candidate.DeliveryPointId,
					IsSelfDelivery = candidate.IsSelfDelivery,
					DeliveryPointAddress = deliveryPointAddress?.Substring(0, Math.Min(deliveryPointAddress.Length, 1000)),
					PhoneNumber = candidate.ContactPhoneNumber,
					EmailAddress = PriorityEmailAddressSelector.SelectPriorityEmailAddress(counterpartyEmails),
					LastOrderId = candidate.OrderId,
					LastOrderDeliveryDate = candidate.DeliveryDate.Value
				};

				lastServiceOrders.Add(lastServiceOrder);
			}

			return lastServiceOrders;
		}

		private static LastServiceOrderDto CreateLastServiceOrderDto(LastServiceOrder lastServiceOrder) =>
			new LastServiceOrderDto
			{
				LastServiceOrderId = lastServiceOrder.Id,
				CounterpartyId = lastServiceOrder.CounterpartyId,
				CounterpartyName = lastServiceOrder.CounterpartyName,
				DeliveryPointAddress = lastServiceOrder.DeliveryPointAddress,
				PhoneNumber = lastServiceOrder.PhoneNumber,
				EmailAddress = lastServiceOrder.EmailAddress,
				LastOrderDeliveryDate = lastServiceOrder.LastOrderDeliveryDate
			};
	}
}
