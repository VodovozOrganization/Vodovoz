using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Contracts.Dto;
using BitrixNotificationsSend.Library.Extensions;
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
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Notifications;
using Vodovoz.Settings.Organizations;

namespace BitrixNotificationsSend.Library.Services
{
	/// <inheritdoc/>
	public class ChainStoreDelayDealsCreateService : IChainStoreDelayDealsCreateService
	{
		private readonly ILogger<ChainStoreDelayDealsCreateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly ICounterpartySettings _counterpartySettings;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;
		private readonly IBitrixDealsClient _bitrixDealsClient;
		private readonly IBitrixBatchesSendService _bitrixBatchesSendService;
		private readonly IGenericRepository<ChainStoreDelayBitrixDeal> _dealRepository;
		private readonly IChainStoreDelayBitrixDealFactory _dealFactory;
		private readonly IBitrixDealsStagesReadService _bitrixDealsStagesReadService;

		/// <summary>
		/// Максимальное количество идентификаторов в одном запросе к базе данных
		/// </summary>
		private const int _maxIdsInQuery = 500;

		public ChainStoreDelayDealsCreateService(
			ILogger<ChainStoreDelayDealsCreateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			ICounterpartySettings counterpartySettings,
			IOrganizationSettings organizationSettings,
			IBitrixNotificationsSendSettings bitrixNotificationsSendSettings,
			IBitrixDealsClient bitrixDealsClient,
			IBitrixBatchesSendService bitrixBatchesSendService,
			IGenericRepository<ChainStoreDelayBitrixDeal> dealRepository,
			IChainStoreDelayBitrixDealFactory dealFactory,
			IBitrixDealsStagesReadService bitrixDealsStagesReadService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings
				?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
			_bitrixDealsClient = bitrixDealsClient ?? throw new ArgumentNullException(nameof(bitrixDealsClient));
			_bitrixBatchesSendService = bitrixBatchesSendService ?? throw new ArgumentNullException(nameof(bitrixBatchesSendService));
			_dealRepository = dealRepository ?? throw new ArgumentNullException(nameof(dealRepository));
			_dealFactory = dealFactory ?? throw new ArgumentNullException(nameof(dealFactory));
			_bitrixDealsStagesReadService = bitrixDealsStagesReadService
				?? throw new ArgumentNullException(nameof(bitrixDealsStagesReadService));
		}

		/// <inheritdoc/>
		public async Task<int> CollectDeals(CancellationToken cancellationToken)
		{
			var moscowNow = DateTime.UtcNow.ToMoscowDateTime();
			var today = moscowNow.Date;
			var daysBeforePaymentDate = _bitrixNotificationsSendSettings.ChainStoreDelayDealsDaysToPaymentDate;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(ChainStoreDelayDealsCreateService)}. Сбор сделок по отсрочке сети"))
			{
				var orders = await _orderRepository.GetChainStoreDelayOrdersAsync(
					uow,
					today,
					daysBeforePaymentDate,
					_counterpartySettings.CounterpartyFromTenderId,
					_organizationSettings.VodovozOrganizationId,
					cancellationToken);

				var candidateOrderIds = orders
					.Select(x => x.OrderId)
					.ToArray();

				if(candidateOrderIds.Length == 0)
				{
					_logger.LogInformation(
						"Нет заказов в окне для сделок по отсрочке сети в Битрикс24");
					return 0;
				}

				var previouslySentOrderIds = new HashSet<int>(_dealRepository
					.Get(uow, x => candidateOrderIds.Contains(x.OrderId))
					.Select(x => x.OrderId));

				var deals = _dealFactory
					.CreateDeals(orders, previouslySentOrderIds, moscowNow)
					.ToList();

				if(deals.Count > 0)
				{
					foreach(var deal in deals)
					{
						await uow.SaveAsync(deal, cancellationToken: cancellationToken);
					}

					await uow.CommitAsync(cancellationToken);
				}

				_logger.LogInformation(
					"Сбор сделок по отсрочке сети в Битрикс24: заказов в окне {OrdersCount}, создано отправок {DealsCount}",
					orders.Count,
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

				var deals = _dealRepository
					.Get(uow, x => x.Status == ChainStoreDelayBitrixDealStatus.DealCreateRequired)
					.ToList();

				var paymentStatuses = await GetOrdersPaymentStatuses(uow, deals.Select(x => x.OrderId), cancellationToken);

				var paidDeals = deals
					.Where(x => paymentStatuses.TryGetValue(x.OrderId, out var paymentStatus) && paymentStatus == OrderPaymentStatus.Paid)
					.ToList();

				if(paidDeals.Count > 0)
				{
					var now = DateTime.UtcNow.ToMoscowDateTime();

					foreach(var paidDeal in paidDeals)
					{
						paidDeal.Status = ChainStoreDelayBitrixDealStatus.NotSentOrderPaid;
						paidDeal.LastUpdateDate = now;

						await uow.SaveAsync(paidDeal, cancellationToken: cancellationToken);
					}

					await uow.CommitAsync(cancellationToken);

					_logger.LogInformation(
						"Заказы оплачены до отправки, сделки по отсрочке сети не создаются. Количество отправок: {PaidDealsCount}",
						paidDeals.Count);
				}

				dealDtos = deals
					.Except(paidDeals)
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

			await SaveDealErrors(sendResult.Errors, ChainStoreDelayDealCommandKeys.CreateCommandKeyPrefix, cancellationToken);

			_logger.LogInformation(
				"Успешно создано {SuccessfulDealsCount} сделок по отсрочке сети из запланированных {PlannedDealsCount}",
				sendResult.SuccessfulCount,
				dealDtos.Count);
		}

		/// <inheritdoc/>
		public async Task CloseDealsOfPaidOrders(CancellationToken cancellationToken)
		{
			var dealCloses = await GetPaidOrdersDealCloses(cancellationToken);

			if(dealCloses.Count == 0)
			{
				_logger.LogInformation("Нет сделок оплаченных заказов по отсрочке сети для закрытия в Битрикс24");
				return;
			}

			_logger.LogInformation(
				"Начало закрытия сделок оплаченных заказов по отсрочке сети в Битрикс24. Найдено оплаченных: {PaidDealsCount}",
				dealCloses.Count);

			var dealClosesToSend = await GetDealClosesToSendByBitrixStages(dealCloses, cancellationToken);

			if(dealClosesToSend.Count == 0)
			{
				return;
			}

			await SendDealCloses(dealClosesToSend, cancellationToken);
		}

		private async Task<List<ChainStoreDelayDealCloseDto>> GetPaidOrdersDealCloses(CancellationToken cancellationToken)
		{
			var closedStageId = _bitrixNotificationsSendSettings.ChainStoreDelayDealsClosedStageId;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(ChainStoreDelayDealsCreateService)}. Поиск сделок оплаченных заказов по отсрочке сети"))
			{
				var createdDeals = _dealRepository
					.Get(uow, x => x.Status == ChainStoreDelayBitrixDealStatus.DealCreated && x.BitrixDealId != null)
					.ToList();

				var paymentStatuses = await GetOrdersPaymentStatuses(uow, createdDeals.Select(x => x.OrderId), cancellationToken);

				return createdDeals
					.Where(x => paymentStatuses.TryGetValue(x.OrderId, out var paymentStatus) && paymentStatus == OrderPaymentStatus.Paid)
					.Select(x => new ChainStoreDelayDealCloseDto
					{
						ChainStoreDelayBitrixDealId = x.Id,
						BitrixDealId = x.BitrixDealId.Value,
						StageId = closedStageId
					})
					.ToList();
			}
		}

		private async Task<List<ChainStoreDelayDealCloseDto>> GetDealClosesToSendByBitrixStages(
			IReadOnlyList<ChainStoreDelayDealCloseDto> dealCloses,
			CancellationToken cancellationToken)
		{
			var dealsStages = await _bitrixDealsStagesReadService.GetDealsStages(
				dealCloses.Select(x => x.BitrixDealId),
				cancellationToken);

			var finalStageIds = new HashSet<string>(
				_bitrixNotificationsSendSettings.ChainStoreDelayDealsFinalStageIds,
				StringComparer.OrdinalIgnoreCase);

			var notFoundDealIds = new List<int>();
			var closedInBitrixDealIds = new List<int>();
			var dealClosesToSend = new List<ChainStoreDelayDealCloseDto>();

			foreach(var dealClose in dealCloses)
			{
				if(dealsStages.NotFoundDealIds.Contains(dealClose.BitrixDealId))
				{
					notFoundDealIds.Add(dealClose.ChainStoreDelayBitrixDealId);
					continue;
				}

				if(!dealsStages.StagesByDealIds.TryGetValue(dealClose.BitrixDealId, out var currentStageId))
				{
					continue;
				}

				if(finalStageIds.Contains(currentStageId))
				{
					closedInBitrixDealIds.Add(dealClose.ChainStoreDelayBitrixDealId);
					continue;
				}

				dealClosesToSend.Add(dealClose);
			}

			await MarkDealsStatus(notFoundDealIds, ChainStoreDelayBitrixDealStatus.DealNotFound, cancellationToken);
			await MarkDealsStatus(closedInBitrixDealIds, ChainStoreDelayBitrixDealStatus.DealClosedInBitrix, cancellationToken);

			_logger.LogInformation(
				"Сделок удалено в Битрикс24: {NotFoundDealsCount}, уже находятся в завершённой стадии: {ClosedDealsCount}, " +
				"будет закрыто: {DealClosesToSendCount}",
				notFoundDealIds.Count,
				closedInBitrixDealIds.Count,
				dealClosesToSend.Count);

			return dealClosesToSend;
		}

		private async Task SendDealCloses(
			IReadOnlyList<ChainStoreDelayDealCloseDto> dealClosesToSend,
			CancellationToken cancellationToken)
		{
			var sendResult = await _bitrixBatchesSendService.SendAll(
				dealClosesToSend,
				dealClose => dealClose.DealCommandKey,
				(batchDealCloses, batchCancellationToken) =>
					_bitrixDealsClient.CloseChainStoreDelayDeals(batchDealCloses, batchCancellationToken),
				MarkDealsClosed,
				cancellationToken);

			await SaveDealErrors(sendResult.Errors, ChainStoreDelayDealCommandKeys.CloseCommandKeyPrefix, cancellationToken);

			_logger.LogInformation(
				"Успешно закрыто {SuccessfulDealsCount} сделок по отсрочке сети из запланированных {PlannedDealsCount}",
				sendResult.SuccessfulCount,
				dealClosesToSend.Count);
		}

		private async Task<IDictionary<int, OrderPaymentStatus>> GetOrdersPaymentStatuses(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			CancellationToken cancellationToken)
		{
			var paymentStatuses = new Dictionary<int, OrderPaymentStatus>();

			foreach(var idsChunk in orderIds.Distinct().SplitToChunks(_maxIdsInQuery))
			{
				var chunkStatuses = await _orderRepository.GetOrdersPaymentStatusesAsync(uow, idsChunk, cancellationToken);

				foreach(var chunkStatus in chunkStatuses)
				{
					paymentStatuses[chunkStatus.Key] = chunkStatus.Value;
				}
			}

			return paymentStatuses;
		}

		private async Task MarkDealsClosed(
			IReadOnlyList<ChainStoreDelayDealCloseDto> succeededDealCloses,
			CancellationToken cancellationToken)
		{
			var dealIds = succeededDealCloses
				.Select(x => x.ChainStoreDelayBitrixDealId)
				.ToArray();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(ChainStoreDelayDealsCreateService)}. Обновление статуса закрытых сделок по отсрочке сети"))
			{
				var deals = _dealRepository.Get(uow, x => dealIds.Contains(x.Id));
				var now = DateTime.UtcNow.ToMoscowDateTime();

				foreach(var deal in deals)
				{
					deal.Status = ChainStoreDelayBitrixDealStatus.DealClosedOnPayment;
					deal.ClosedDate = now;
					deal.LastSynchronizedDate = now;
					deal.LastUpdateDate = now;
					deal.LastError = null;

					await uow.SaveAsync(deal, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private async Task MarkDealsStatus(
			IReadOnlyList<int> dealIds,
			ChainStoreDelayBitrixDealStatus status,
			CancellationToken cancellationToken)
		{
			if(dealIds.Count == 0)
			{
				return;
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(ChainStoreDelayDealsCreateService)}. Обновление статуса сделок по отсрочке сети"))
			{
				var now = DateTime.UtcNow.ToMoscowDateTime();

				foreach(var idsChunk in dealIds.SplitToChunks(_maxIdsInQuery))
				{
					var ids = idsChunk.ToArray();

					var deals = _dealRepository.Get(uow, x => ids.Contains(x.Id));

					foreach(var deal in deals)
					{
						deal.Status = status;
						deal.ClosedDate = now;
						deal.LastUpdateDate = now;

						await uow.SaveAsync(deal, cancellationToken: cancellationToken);
					}
				}

				await uow.CommitAsync(cancellationToken);
			}
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
					deal.Status = ChainStoreDelayBitrixDealStatus.DealCreated;
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
			string commandKeyPrefix,
			CancellationToken cancellationToken)
		{
			var errors = dealErrors?.ToArray();

			if(errors == null || errors.Length == 0)
			{
				return;
			}

			var dealIdsByCommandKey = errors
				.Select(x => new { x.CommandKey, DealId = GetDealIdFromCommandKey(x.CommandKey, commandKeyPrefix) })
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
				DeliveryAddress = deal.DeliveryAddress,
				OrderId = deal.OrderId,
				OrderAuthor = deal.OrderAuthor,
				OrderSum = deal.OrderSum,
				EstimatedPaymentDate = deal.EstimatedPaymentDate,
				DaysOverdue = deal.DaysOverdue,
				Debt = deal.Debt
			};

		private static int? GetDealIdFromCommandKey(string commandKey, string commandKeyPrefix)
		{
			if(string.IsNullOrWhiteSpace(commandKey) || !commandKey.StartsWith(commandKeyPrefix, StringComparison.Ordinal))
			{
				return null;
			}

			return int.TryParse(commandKey.Substring(commandKeyPrefix.Length), out var dealId)
				? (int?)dealId
				: null;
		}
	}
}
