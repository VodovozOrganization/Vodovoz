using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Contracts.Dto;
using BitrixNotificationsSend.Library.Services.Batches;
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

namespace BitrixNotificationsSend.Library.Services
{
	/// <summary>
	/// Сервис подготовки и создания сделок в Битрикс24 по недовозам.
	/// </summary>
	public class UndeliveredOrdersDealsCreateService : IUndeliveredOrdersDealsCreateService
	{
		private readonly ILogger<UndeliveredOrdersDealsCreateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IBitrixDealsClient _bitrixDealsClient;
		private readonly IBitrixBatchesSendService _bitrixBatchesSendService;
		private readonly IGenericRepository<UndeliveredOrderBitrixDeal> _undeliveredOrderBitrixDealRepository;

		public UndeliveredOrdersDealsCreateService(
			ILogger<UndeliveredOrdersDealsCreateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IBitrixDealsClient bitrixDealsClient,
			IBitrixBatchesSendService bitrixBatchesSendService,
			IGenericRepository<UndeliveredOrderBitrixDeal> undeliveredOrderBitrixDealRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_bitrixDealsClient = bitrixDealsClient ?? throw new ArgumentNullException(nameof(bitrixDealsClient));
			_bitrixBatchesSendService = bitrixBatchesSendService ?? throw new ArgumentNullException(nameof(bitrixBatchesSendService));
			_undeliveredOrderBitrixDealRepository = undeliveredOrderBitrixDealRepository
				?? throw new ArgumentNullException(nameof(undeliveredOrderBitrixDealRepository));
		}

		/// <summary>
		/// Собирает новые и измененные недовозы в локальное состояние синхронизации с Битрикс24.
		/// </summary>
		/// <param name="minLastEditedTime">Минимальное время изменения недовоза для попадания в сбор.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Количество созданных или обновленных записей синхронизации.</returns>
		public async Task<int> CollectUndeliveredOrders(DateTime minLastEditedTime, CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(UndeliveredOrdersDealsCreateService)}. Сбор недовозов для Битрикс24"))
			{
				var undeliveries = uow.Session.Query<UndeliveredOrder>()
					.Where(x => x.Id > 0 && x.LastEditedTime >= minLastEditedTime)
					.ToList();

				if(!undeliveries.Any())
				{
					_logger.LogInformation("Нет новых или измененных недовозов для подготовки к отправке в Битрикс24");
					return 0;
				}

				var undeliveryIds = undeliveries
					.Select(x => x.Id)
					.ToArray();

				var synchronizationStates = _undeliveredOrderBitrixDealRepository
					.Get(uow, x => undeliveryIds.Contains(x.UndeliveredOrderId))
					.ToDictionary(x => x.UndeliveredOrderId);

				var now = DateTime.Now;
				var changedCount = 0;

				foreach(var undelivery in undeliveries)
				{
					cancellationToken.ThrowIfCancellationRequested();

					if(synchronizationStates.TryGetValue(undelivery.Id, out var synchronizationState)
						&& synchronizationState.SourceLastEditedTime >= undelivery.LastEditedTime)
					{
						continue;
					}

					var actualState = CreateActualState(uow, undelivery, synchronizationState, now);

					await uow.SaveAsync(actualState, cancellationToken: cancellationToken);
					changedCount++;
				}

				if(changedCount == 0)
				{
					_logger.LogInformation("Нет новых или измененных недовозов для подготовки к отправке в Битрикс24");
					return 0;
				}

				await uow.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Подготовлено {UndeliveriesCount} новых или измененных недовозов для отправки в Битрикс24",
					changedCount);

				return changedCount;
			}
		}

		/// <summary>
		/// Отправляет в Битрикс24 запросы на создание сделок по недовозам со статусом "Требуется создание сделки".
		/// После успешного создания сохраняет идентификатор сделки Битрикс24.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены.</param>
		public async Task SendNotCreatedDeals(CancellationToken cancellationToken)
		{
			List<UndeliveredOrderDto> undeliveredOrderDtos;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(UndeliveredOrdersDealsCreateService)}. Поиск не созданных сделок по недовозам"))
			{
				undeliveredOrderDtos = _undeliveredOrderBitrixDealRepository
					.Get(uow, x => x.Status == BitrixDealSynchronizationStatus.DealCreateRequired)
					.Select(CreateUndeliveredOrderDto)
					.ToList();
			}

			if(!undeliveredOrderDtos.Any())
			{
				_logger.LogInformation("Нет недовозов для создания сделок в Битрикс24");
				return;
			}

			_logger.LogInformation(
				"Начало создания сделок по недовозам в Битрикс24. Количество строк: {UndeliveredOrdersCount}",
				undeliveredOrderDtos.Count);

			var sendResult = await _bitrixBatchesSendService.SendAll(
				undeliveredOrderDtos,
				undeliveredOrderDto => undeliveredOrderDto.DealCommandKey,
				(batchUndeliveredOrders, batchCancellationToken) =>
					_bitrixDealsClient.SendUndeliveredOrderDeals(batchUndeliveredOrders, batchCancellationToken),
				MarkDealsCreated,
				cancellationToken);

			await SaveDealErrors(sendResult.Errors, cancellationToken);

			_logger.LogInformation(
				"Успешно создано {SuccessfulDealsCount} сделок по недовозам из запланированных {PlannedDealsCount}",
				sendResult.SuccessfulCount,
				undeliveredOrderDtos.Count);
		}

		/// <summary>
		/// Отправляет в Битрикс24 запросы на обновление созданных сделок по измененным недовозам.
		/// После успешного обновления помечает сделку актуальной.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены.</param>
		public async Task SendNotActualDeals(CancellationToken cancellationToken)
		{
			List<UndeliveredOrderDto> undeliveredOrderDtos;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(UndeliveredOrdersDealsCreateService)}. Поиск не актуальных сделок по недовозам"))
			{
				undeliveredOrderDtos = _undeliveredOrderBitrixDealRepository
					.Get(uow, x => x.Status == BitrixDealSynchronizationStatus.DealUpdateRequired && x.BitrixDealId.HasValue)
					.Select(CreateUndeliveredOrderDto)
					.ToList();
			}

			if(!undeliveredOrderDtos.Any())
			{
				_logger.LogInformation("Нет недовозов для обновления сделок в Битрикс24");
				return;
			}

			_logger.LogInformation(
				"Начало обновления сделок по недовозам в Битрикс24. Количество строк: {UndeliveredOrdersCount}",
				undeliveredOrderDtos.Count);

			var sendResult = await _bitrixBatchesSendService.SendAll(
				undeliveredOrderDtos,
				undeliveredOrderDto => undeliveredOrderDto.DealCommandKey,
				(batchUndeliveredOrders, batchCancellationToken) =>
					_bitrixDealsClient.UpdateUndeliveredOrderDeals(batchUndeliveredOrders, batchCancellationToken),
				MarkDealsActual,
				cancellationToken);

			await SaveDealErrors(sendResult.Errors, cancellationToken);

			_logger.LogInformation(
				"Успешно обновлено {SuccessfulDealsCount} сделок по недовозам из запланированных {PlannedDealsCount}",
				sendResult.SuccessfulCount,
				undeliveredOrderDtos.Count);
		}

		private async Task MarkDealsCreated(
			IReadOnlyList<UndeliveredOrderDto> succeededUndeliveredOrders,
			IDictionary<string, long> createdDealIdsByCommandKey,
			CancellationToken cancellationToken)
		{
			var synchronizationStateIds = succeededUndeliveredOrders
				.Select(x => x.UndeliveredOrderBitrixDealId)
				.ToArray();

			var succeededUndeliveredOrdersByStateId = succeededUndeliveredOrders
				.ToDictionary(x => x.UndeliveredOrderBitrixDealId);

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(UndeliveredOrdersDealsCreateService)}. Обновление статуса сделок по недовозам"))
			{
				var synchronizationStates = _undeliveredOrderBitrixDealRepository
					.Get(uow, x => synchronizationStateIds.Contains(x.Id));

				var now = DateTime.Now;

				foreach(var synchronizationState in synchronizationStates)
				{
					if(!succeededUndeliveredOrdersByStateId.TryGetValue(synchronizationState.Id, out var succeededUndeliveredOrder)
						|| !createdDealIdsByCommandKey.TryGetValue(succeededUndeliveredOrder.DealCommandKey, out var bitrixDealId))
					{
						continue;
					}

					synchronizationState.BitrixDealId = bitrixDealId;
					synchronizationState.Status = BitrixDealSynchronizationStatus.DealActual;
					synchronizationState.LastSynchronizedDate = now;
					synchronizationState.LastError = null;

					await uow.SaveAsync(synchronizationState, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private async Task MarkDealsActual(
			IReadOnlyList<UndeliveredOrderDto> succeededUndeliveredOrders,
			CancellationToken cancellationToken)
		{
			var synchronizationStateIds = succeededUndeliveredOrders
				.Select(x => x.UndeliveredOrderBitrixDealId)
				.ToArray();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(UndeliveredOrdersDealsCreateService)}. Обновление статуса актуальных сделок по недовозам"))
			{
				var synchronizationStates = _undeliveredOrderBitrixDealRepository
					.Get(uow, x => synchronizationStateIds.Contains(x.Id));

				var now = DateTime.Now;

				foreach(var synchronizationState in synchronizationStates)
				{
					synchronizationState.Status = BitrixDealSynchronizationStatus.DealActual;
					synchronizationState.LastSynchronizedDate = now;
					synchronizationState.LastError = null;

					await uow.SaveAsync(synchronizationState, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private async Task SaveDealErrors(
			IEnumerable<BitrixBatchItemError> dealErrors,
			CancellationToken cancellationToken)
		{
			var errors = dealErrors?.ToArray();

			if(errors == null || !errors.Any())
			{
				return;
			}

			var synchronizationStateIdsByCommandKey = errors
				.Select(x => new
				{
					x.CommandKey,
					SynchronizationStateId = GetSynchronizationStateIdFromCommandKey(x.CommandKey)
				})
				.Where(x => x.SynchronizationStateId.HasValue)
				.ToDictionary(x => x.CommandKey, x => x.SynchronizationStateId.Value);

			var synchronizationStateIds = synchronizationStateIdsByCommandKey.Values.ToArray();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(UndeliveredOrdersDealsCreateService)}. Сохранение ошибок сделок по недовозам"))
			{
				var synchronizationStates = _undeliveredOrderBitrixDealRepository
					.Get(uow, x => synchronizationStateIds.Contains(x.Id))
					.ToDictionary(x => x.Id);

				foreach(var error in errors)
				{
					if(!synchronizationStateIdsByCommandKey.TryGetValue(error.CommandKey, out var synchronizationStateId)
						|| !synchronizationStates.TryGetValue(synchronizationStateId, out var synchronizationState))
					{
						continue;
					}

					synchronizationState.LastError = string.IsNullOrWhiteSpace(error.ErrorCode)
						? error.Message
						: $"{error.ErrorCode}: {error.Message}";
					synchronizationState.LastUpdateDate = DateTime.Now;

					await uow.SaveAsync(synchronizationState, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private UndeliveredOrderBitrixDeal CreateActualState(
			IUnitOfWork uow,
			UndeliveredOrder undelivery,
			UndeliveredOrderBitrixDeal existingState,
			DateTime now)
		{
			const string selfDeliveryAddress = "Самовывоз";

			var state = existingState ?? new UndeliveredOrderBitrixDeal
			{
				CreationDate = now,
				UndeliveredOrderId = undelivery.Id
			};

			var oldOrder = undelivery.OldOrder;
			var routeLists = oldOrder == null
				? new List<Vodovoz.Domain.Logistic.RouteList>()
				: _orderRepository.GetAllRLForOrder(uow, oldOrder).ToList();

			state.LastUpdateDate = now;
			state.Status = state.BitrixDealId.HasValue
				? BitrixDealSynchronizationStatus.DealUpdateRequired
				: BitrixDealSynchronizationStatus.DealCreateRequired;
			state.SourceLastEditedTime = undelivery.LastEditedTime;
			state.DealTitle = oldOrder == null ? $"Недовоз №{undelivery.Id}" : $"Недовоз по заказу №{oldOrder.Id}";
			state.OrderId = oldOrder?.Id.ToString();
			state.CounterpartyName = oldOrder?.Client?.Name;
			state.DeliveryAddress = oldOrder == null
				? null
				: oldOrder.SelfDelivery
					? selfDeliveryAddress
					: oldOrder.DeliveryPoint?.ShortAddress;
			state.DeliveryDate = oldOrder?.DeliveryDate;
			state.DeliveryInterval = oldOrder == null
				? null
				: oldOrder.SelfDelivery
					? selfDeliveryAddress
					: oldOrder.DeliverySchedule?.Name;
			state.DriverName = string.Join(", ", routeLists
				.Select(x => x.Driver?.ShortName)
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct());
			state.RouteListId = string.Join(", ", routeLists
				.Select(x => x.Id.ToString())
				.Distinct());
			state.TypeOfUndelivery = undelivery.UndeliveryDetalization?.UndeliveryKind?.Name;
			state.SpecificationOfUndelivery = undelivery.UndeliveryDetalization?.Name;
			state.Comment = undelivery.Reason;
			state.NewOrderId = undelivery.NewOrder?.Id.ToString();
			state.LastError = null;

			return state;
		}

		private static UndeliveredOrderDto CreateUndeliveredOrderDto(UndeliveredOrderBitrixDeal synchronizationState) =>
			new UndeliveredOrderDto
			{
				UndeliveredOrderBitrixDealId = synchronizationState.Id,
				BitrixDealId = synchronizationState.BitrixDealId,
				Title = synchronizationState.DealTitle,
				OrderId = synchronizationState.OrderId,
				CounterpartyName = synchronizationState.CounterpartyName,
				DeliveryAddress = synchronizationState.DeliveryAddress,
				DeliveryDate = synchronizationState.DeliveryDate,
				DeliveryInterval = synchronizationState.DeliveryInterval,
				DriverName = synchronizationState.DriverName,
				RouteListId = synchronizationState.RouteListId,
				TypeOfUndelivery = synchronizationState.TypeOfUndelivery,
				SpecificationOfUndelivery = synchronizationState.SpecificationOfUndelivery,
				Comment = synchronizationState.Comment,
				NewOrderId = synchronizationState.NewOrderId
			};

		private static int? GetSynchronizationStateIdFromCommandKey(string commandKey)
		{
			const string commandKeyPrefix = "undelivered_order_deal_";

			if(string.IsNullOrWhiteSpace(commandKey) || !commandKey.StartsWith(commandKeyPrefix, StringComparison.Ordinal))
			{
				return null;
			}

			return int.TryParse(commandKey.Substring(commandKeyPrefix.Length), out var synchronizationStateId)
				? (int?)synchronizationStateId
				: null;
		}
	}
}
