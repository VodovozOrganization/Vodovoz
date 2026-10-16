using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Contracts.Dto;
using BitrixNotificationsSend.Library.Factories;
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
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Library.Services
{
	/// <inheritdoc/>
	public class ChainStoreDelayDealsCreateService : IChainStoreDelayDealsCreateService
	{
		private const string _dealCommandKeyPrefix = "chain_store_delay_deal_";

		private readonly ILogger<ChainStoreDelayDealsCreateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly ICounterpartySettings _counterpartySettings;
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;
		private readonly IBitrixDealsClient _bitrixDealsClient;
		private readonly IBitrixBatchesSendService _bitrixBatchesSendService;
		private readonly IGenericRepository<ChainStoreDelayBitrixDeal> _dealRepository;
		private readonly IGenericRepository<ChainStoreDelayBitrixDealOrder> _dealOrderRepository;
		private readonly IChainStoreDelayBitrixDealFactory _dealFactory;

		public ChainStoreDelayDealsCreateService(
			ILogger<ChainStoreDelayDealsCreateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			ICounterpartySettings counterpartySettings,
			IBitrixNotificationsSendSettings bitrixNotificationsSendSettings,
			IBitrixDealsClient bitrixDealsClient,
			IBitrixBatchesSendService bitrixBatchesSendService,
			IGenericRepository<ChainStoreDelayBitrixDeal> dealRepository,
			IGenericRepository<ChainStoreDelayBitrixDealOrder> dealOrderRepository,
			IChainStoreDelayBitrixDealFactory dealFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings
				?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
			_bitrixDealsClient = bitrixDealsClient ?? throw new ArgumentNullException(nameof(bitrixDealsClient));
			_bitrixBatchesSendService = bitrixBatchesSendService ?? throw new ArgumentNullException(nameof(bitrixBatchesSendService));
			_dealRepository = dealRepository ?? throw new ArgumentNullException(nameof(dealRepository));
			_dealOrderRepository = dealOrderRepository ?? throw new ArgumentNullException(nameof(dealOrderRepository));
			_dealFactory = dealFactory ?? throw new ArgumentNullException(nameof(dealFactory));
		}

		/// <inheritdoc/>
		public async Task<int> CollectDeals(CancellationToken cancellationToken)
		{
			var moscowNow = DateTime.UtcNow.ToMoscowDateTime();
			var today = moscowNow.Date;
			var daysBeforePaymentDate = _bitrixNotificationsSendSettings.ChainStoreDelayDealsDaysBeforePaymentDate;
			var maxDate = today.AddDays(daysBeforePaymentDate);

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(ChainStoreDelayDealsCreateService)}. Сбор сделок по отсрочке сети"))
			{
				var orders = await _orderRepository.GetChainStoreDelayOrdersAsync(
					uow,
					maxDate,
					_counterpartySettings.CounterpartyFromTenderId,
					cancellationToken);

				var candidateOrderIds = orders
					.Where(x => x.EstimatedPaymentDate > today && x.EstimatedPaymentDate <= maxDate)
					.Select(x => x.OrderId)
					.ToArray();

				if(candidateOrderIds.Length == 0)
				{
					_logger.LogInformation(
						"Нет заказов-кандидатов для сделок по отсрочке сети в Битрикс24. Заказов в выборке: {OrdersCount}",
						orders.Count);
					return 0;
				}

				var previouslySentOrderIds = new HashSet<int>(_dealOrderRepository
					.Get(uow, x => candidateOrderIds.Contains(x.OrderId))
					.Select(x => x.OrderId));

				var counterpartyIdsWithTodayDeals = new HashSet<int>(_dealRepository
					.Get(uow, x => x.CreationDate >= today && x.CreationDate < today.AddDays(1))
					.Select(x => x.CounterpartyId));

				var deals = _dealFactory
					.CreateDeals(orders, previouslySentOrderIds, counterpartyIdsWithTodayDeals, today, daysBeforePaymentDate, moscowNow)
					.ToList();

				foreach(var deal in deals)
				{
					await uow.SaveAsync(deal, cancellationToken: cancellationToken);
				}

				if(deals.Count > 0)
				{
					await uow.CommitAsync(cancellationToken);
				}

				_logger.LogInformation(
					"Сбор сделок по отсрочке сети в Битрикс24: заказов в выборке {OrdersCount}, кандидатов {CandidatesCount}, создано отправок {DealsCount}",
					orders.Count,
					candidateOrderIds.Length,
					deals.Count);

				return deals.Count;
			}
		}

		/// <inheritdoc/>
		public async Task SendNotCreatedDeals(CancellationToken cancellationToken)
		{
			List<ChainStoreDelayDealDto> dealDtos;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(ChainStoreDelayDealsCreateService)}. Поиск не созданных сделок по отсрочке сети"))
			{
				var categoryId = _bitrixNotificationsSendSettings.ChainStoreDelayDealsCategoryId;
				var stageId = _bitrixNotificationsSendSettings.ChainStoreDelayDealsStageId;

				dealDtos = _dealRepository
					.Get(uow, x => x.Status == BitrixDealSynchronizationStatus.DealCreateRequired)
					.Select(x => CreateDealDto(x, categoryId, stageId))
					.ToList();
			}

			if(dealDtos.Count == 0)
			{
				_logger.LogInformation("Нет отправок для создания сделок по отсрочке сети в Битрикс24");
				return;
			}

			_logger.LogInformation(
				"Начало создания сделок по отсрочке сети в Битрикс24. Количество сделок: {DealsCount}",
				dealDtos.Count);

			var sendResult = await _bitrixBatchesSendService.SendAll(
				dealDtos,
				dealDto => dealDto.DealCommandKey,
				(batchDeals, batchCancellationToken) =>
					_bitrixDealsClient.SendChainStoreDelayDeals(batchDeals, batchCancellationToken),
				MarkDealsCreated,
				cancellationToken);

			await SaveDealErrors(sendResult.Errors, cancellationToken);

			_logger.LogInformation(
				"Успешно создано {SuccessfulDealsCount} сделок по отсрочке сети из запланированных {PlannedDealsCount}",
				sendResult.SuccessfulCount,
				dealDtos.Count);
		}

		private async Task MarkDealsCreated(
			IReadOnlyList<ChainStoreDelayDealDto> succeededDeals,
			IDictionary<string, long> createdDealIdsByCommandKey,
			CancellationToken cancellationToken)
		{
			var dealIds = succeededDeals
				.Select(x => x.ChainStoreDelayBitrixDealId)
				.ToArray();

			var succeededDealsById = succeededDeals
				.ToDictionary(x => x.ChainStoreDelayBitrixDealId);

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(ChainStoreDelayDealsCreateService)}. Обновление статуса сделок по отсрочке сети"))
			{
				var deals = _dealRepository.Get(uow, x => dealIds.Contains(x.Id));
				var now = DateTime.UtcNow.ToMoscowDateTime();

				foreach(var deal in deals)
				{
					if(!succeededDealsById.TryGetValue(deal.Id, out var succeededDeal)
						|| !createdDealIdsByCommandKey.TryGetValue(succeededDeal.DealCommandKey, out var bitrixDealId))
					{
						continue;
					}

					deal.BitrixDealId = bitrixDealId;
					deal.Status = BitrixDealSynchronizationStatus.DealActual;
					deal.LastSynchronizedDate = now;
					deal.LastUpdateDate = now;
					deal.LastError = null;

					await uow.SaveAsync(deal, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private async Task SaveDealErrors(
			IEnumerable<BitrixBatchItemError> dealErrors,
			CancellationToken cancellationToken)
		{
			var errors = dealErrors?.ToArray();

			if(errors == null || errors.Length == 0)
			{
				return;
			}

			var dealIdsByCommandKey = errors
				.Select(x => new { x.CommandKey, DealId = GetDealIdFromCommandKey(x.CommandKey) })
				.Where(x => x.DealId.HasValue)
				.GroupBy(x => x.CommandKey)
				.ToDictionary(x => x.Key, x => x.First().DealId.Value);

			var dealIds = dealIdsByCommandKey.Values.ToArray();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(ChainStoreDelayDealsCreateService)}. Сохранение ошибок сделок по отсрочке сети"))
			{
				var deals = _dealRepository
					.Get(uow, x => dealIds.Contains(x.Id))
					.ToDictionary(x => x.Id);

				var now = DateTime.UtcNow.ToMoscowDateTime();

				foreach(var error in errors)
				{
					if(!dealIdsByCommandKey.TryGetValue(error.CommandKey, out var dealId)
						|| !deals.TryGetValue(dealId, out var deal))
					{
						continue;
					}

					deal.LastError = string.IsNullOrWhiteSpace(error.ErrorCode)
						? error.Message
						: $"{error.ErrorCode}: {error.Message}";
					deal.LastUpdateDate = now;

					await uow.SaveAsync(deal, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private static ChainStoreDelayDealDto CreateDealDto(ChainStoreDelayBitrixDeal deal, int categoryId, string stageId) =>
			new ChainStoreDelayDealDto
			{
				ChainStoreDelayBitrixDealId = deal.Id,
				CategoryId = categoryId,
				StageId = stageId,
				Title = deal.DealTitle,
				CounterpartyName = deal.CounterpartyName,
				PhoneNumber = deal.PhoneNumber,
				CounterpartyInn = deal.CounterpartyInn,
				DeliveryAddresses = deal.DeliveryAddresses,
				InitiatorOrderId = deal.InitiatorOrderId,
				OrderAuthors = deal.OrderAuthors,
				OrdersSum = deal.OrdersSum,
				InitiatorEstimatedPaymentDate = deal.InitiatorEstimatedPaymentDate,
				DaysOverdue = deal.DaysOverdue,
				DebtSum = deal.DebtSum
			};

		private static int? GetDealIdFromCommandKey(string commandKey)
		{
			if(string.IsNullOrWhiteSpace(commandKey) || !commandKey.StartsWith(_dealCommandKeyPrefix, StringComparison.Ordinal))
			{
				return null;
			}

			return int.TryParse(commandKey.Substring(_dealCommandKeyPrefix.Length), out var dealId)
				? (int?)dealId
				: null;
		}
	}
}
