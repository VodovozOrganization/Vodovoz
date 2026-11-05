using BitrixNotificationsSend.Contracts;
using BitrixNotificationsSend.Contracts.Dto;
using BitrixNotificationsSend.Library.Extensions;
using DateTimeHelpers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;

namespace BitrixNotificationsSend.Library.Services
{
	/// <summary>
	/// Обновление созданных сделок по планируемым заказам данными заказов,
	/// созданных клиентами после создания сделок
	/// </summary>
	public partial class PlannedOrdersDealsCreateService
	{
		/// <summary>
		/// Максимальное количество идентификаторов в одном запросе к базе данных
		/// </summary>
		private const int _maxIdsInQuery = 500;

		/// <summary>
		/// Поиск заказов, созданных клиентами начиная с даты последней проверки
		/// Планируемые заказы, по которым найден созданный заказ, переводятся
		/// в стадию "Требуется обновление сделки".
		/// Дата последней проверки сдвигается только после успешного сохранения найденных данных
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		public async Task CollectCreatedOrders(CancellationToken cancellationToken)
		{
			var fromCreateDate = _bitrixNotificationsSendSettings.PlannedOrdersLastOrdersCheckDate;
			var checkStartedAt = DateTime.Today;

			if(checkStartedAt <= fromCreateDate)
			{
				_logger.LogInformation(
					"Проверка созданных заказов не требуется, последняя проверка была {FromCreateDate:yyyy.MM.dd}",
					fromCreateDate);
				return;
			}

			_logger.LogInformation(
				"Начало поиска заказов, созданных клиентами с {FromCreateDate:yyyy.MM.dd}",
				fromCreateDate);

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(PlannedOrdersDealsCreateService)}. Поиск созданных клиентами заказов"))
			{
				var createdOrders = await _orderRepository.GetOrdersCreatedFromDateAsync(
					uow,
					fromCreateDate,
					_canceledOrderStatuses,
					cancellationToken);

				_logger.LogInformation("Найдено {CreatedOrdersCount} созданных заказов", createdOrders.Count);

				var updateRequiredCount = createdOrders.Any()
					? await MarkPlannedOrdersUpdateRequired(uow, createdOrders, cancellationToken)
					: 0;

				_logger.LogInformation(
					"Планируемых заказов, по которым требуется обновление сделки: {UpdateRequiredCount}",
					updateRequiredCount);
			}

			_bitrixNotificationsSendSettings.UpdatePlannedOrdersLastOrdersCheckDate(checkStartedAt);
		}

		private async Task<int> MarkPlannedOrdersUpdateRequired(
			IUnitOfWork uow,
			IList<PlannedOrderCreatedOrderNode> createdOrders,
			CancellationToken cancellationToken)
		{
			var ordersByDeliveryPoints = createdOrders
				.Where(x => !x.IsSelfDelivery && x.DeliveryPointId != null)
				.GroupBy(x => x.DeliveryPointId.Value)
				.ToDictionary(g => g.Key, g => OrderCreatedOrders(g));

			var ordersBySelfDeliveryCounterparties = createdOrders
				.Where(x => x.IsSelfDelivery)
				.GroupBy(x => x.CounterpartyId)
				.ToDictionary(g => g.Key, g => OrderCreatedOrders(g));

			var trackedPlannedOrders = new List<PlannedOrder>();

			foreach(var deliveryPointIds in ordersByDeliveryPoints.Keys.SplitToChunks(_maxIdsInQuery))
			{
				var ids = deliveryPointIds.ToArray();

				trackedPlannedOrders.AddRange(
					_plannedOrderRepository.Get(
						uow,
						x => x.Stage == PlannedOrderBitrixDealStage.DealCreated
							&& x.BitrixDealId != null
							&& !x.IsSelfDelivery
							&& x.DeliveryPointId != null
							&& ids.Contains(x.DeliveryPointId.Value)));
			}

			foreach(var counterpartyIds in ordersBySelfDeliveryCounterparties.Keys.SplitToChunks(_maxIdsInQuery))
			{
				var ids = counterpartyIds.ToArray();

				trackedPlannedOrders.AddRange(
					_plannedOrderRepository.Get(
						uow,
						x => x.Stage == PlannedOrderBitrixDealStage.DealCreated
							&& x.BitrixDealId != null
							&& x.IsSelfDelivery
							&& ids.Contains(x.CounterpartyId)));
			}

			if(!trackedPlannedOrders.Any())
			{
				return 0;
			}

			var now = DateTime.UtcNow.ToMoscowDateTime();
			var updateRequiredCount = 0;

			foreach(var plannedOrder in trackedPlannedOrders)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var createdOrder = FindCreatedOrder(
					plannedOrder,
					ordersByDeliveryPoints,
					ordersBySelfDeliveryCounterparties);

				if(createdOrder == null)
				{
					continue;
				}

				plannedOrder.CreatedOrderId = createdOrder.OrderId;
				plannedOrder.CreatedOrderDeliveryDate = createdOrder.DeliveryDate;
				plannedOrder.Stage = PlannedOrderBitrixDealStage.DealUpdateRequired;
				plannedOrder.LastUpdateDate = now;

				await uow.SaveAsync(plannedOrder, cancellationToken: cancellationToken);

				updateRequiredCount++;
			}

			if(updateRequiredCount > 0)
			{
				await uow.CommitAsync(cancellationToken);
			}

			return updateRequiredCount;
		}

		/// <summary>
		/// Поиск первого заказа, созданного клиентом по планируемому заказу
		/// Учитываются заказы с датой доставки не ранее даты планируемого заказа
		/// </summary>
		/// <param name="plannedOrder">Планируемый заказ</param>
		/// <param name="ordersByDeliveryPoints">Заказы, сгруппированные по точкам доставки</param>
		/// <param name="ordersBySelfDeliveryCounterparties">Заказы, сгруппированные по контрагентам с самовывозом</param>
		/// <returns>Данные заказа, созданного клиентом</returns>
		private static PlannedOrderCreatedOrderNode FindCreatedOrder(
			PlannedOrder plannedOrder,
			IDictionary<int, PlannedOrderCreatedOrderNode[]> ordersByDeliveryPoints,
			IDictionary<int, PlannedOrderCreatedOrderNode[]> ordersBySelfDeliveryCounterparties)
		{
			PlannedOrderCreatedOrderNode[] createdOrders;

			if(plannedOrder.IsSelfDelivery)
			{
				if(!ordersBySelfDeliveryCounterparties.TryGetValue(plannedOrder.CounterpartyId, out createdOrders))
				{
					return null;
				}
			}
			else if(plannedOrder.DeliveryPointId == null
				|| !ordersByDeliveryPoints.TryGetValue(plannedOrder.DeliveryPointId.Value, out createdOrders))
			{
				return null;
			}

			return createdOrders
				.FirstOrDefault(x => x.DeliveryDate >= plannedOrder.PlannedOrderDate);
		}

		private static PlannedOrderCreatedOrderNode[] OrderCreatedOrders(
			IEnumerable<PlannedOrderCreatedOrderNode> createdOrders) =>
			createdOrders
				.OrderBy(x => x.DeliveryDate)
				.ThenBy(x => x.OrderId)
				.ToArray();

		/// <summary>
		/// Отправка в Битрикс24 обновлений сделок по планируемым заказам,
		/// по которым найден созданный клиентом заказ.
		/// Перед обновлением проверяется текущая стадия сделки в Битрикс24:
		/// сделки в завершённых стадиях не обновляются, удалённые сделки снимаются с отслеживания
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		public async Task SendDealsUpdates(CancellationToken cancellationToken)
		{
			List<PlannedOrderDealUpdateDto> dealUpdates;

			var completedStageId = _bitrixNotificationsSendSettings.PlannedOrdersCompletedStageId;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(PlannedOrdersDealsCreateService)}. Поиск сделок для обновления"))
			{
				dealUpdates = _plannedOrderRepository
					.Get(uow, x => x.Stage == PlannedOrderBitrixDealStage.DealUpdateRequired && x.BitrixDealId != null)
					.Select(plannedOrder => CreatePlannedOrderDealUpdateDto(plannedOrder, completedStageId))
					.ToList();
			}

			if(!dealUpdates.Any())
			{
				_logger.LogInformation("Нет сделок по плановым заказам для обновления в Битрикс24");

				return;
			}

			_logger.LogInformation(
				"Начало обновления сделок по плановым заказам в Битрикс24. Количество строк: {DealUpdatesCount}",
				dealUpdates.Count);

			var dealsStages = await _bitrixDealsStagesReadService.GetDealsStages(
				dealUpdates.Select(x => x.BitrixDealId),
				cancellationToken);

			var finalStageIds = new HashSet<string>(
				_bitrixNotificationsSendSettings.PlannedOrdersFinalStageIds,
				StringComparer.OrdinalIgnoreCase);

			var notFoundPlannedOrderIds = new List<int>();
			var closedInBitrixPlannedOrderIds = new List<int>();
			var dealUpdatesToSend = new List<PlannedOrderDealUpdateDto>();

			foreach(var dealUpdate in dealUpdates)
			{
				if(dealsStages.NotFoundDealIds.Contains(dealUpdate.BitrixDealId))
				{
					notFoundPlannedOrderIds.Add(dealUpdate.PlannedOrderId);
					continue;
				}

				if(!dealsStages.StagesByDealIds.TryGetValue(dealUpdate.BitrixDealId, out var stageId))
				{
					continue;
				}

				if(finalStageIds.Contains(stageId))
				{
					closedInBitrixPlannedOrderIds.Add(dealUpdate.PlannedOrderId);
					continue;
				}

				dealUpdatesToSend.Add(dealUpdate);
			}

			await MarkPlannedOrdersStage(
				notFoundPlannedOrderIds,
				PlannedOrderBitrixDealStage.DealNotFound,
				cancellationToken);

			await MarkPlannedOrdersStage(
				closedInBitrixPlannedOrderIds,
				PlannedOrderBitrixDealStage.DealClosedInBitrix,
				cancellationToken);

			_logger.LogInformation(
				"Сделок удалено в Битрикс24: {NotFoundDealsCount}, уже находятся в завершённой стадии: {ClosedDealsCount}, " +
				"будет обновлено: {DealUpdatesToSendCount}",
				notFoundPlannedOrderIds.Count,
				closedInBitrixPlannedOrderIds.Count,
				dealUpdatesToSend.Count);

			if(!dealUpdatesToSend.Any())
			{
				return;
			}

			var sendResult = await _bitrixBatchesSendService.SendAll(
				dealUpdatesToSend,
				dealUpdate => dealUpdate.DealCommandKey,
				(batchDealUpdates, batchCancellationToken) =>
					_bitrixDealsClient.UpdatePlannedOrderDeals(batchDealUpdates, batchCancellationToken),
				MarkDealsCompleted,
				cancellationToken);

			await SaveDealErrors(sendResult.Errors, PlannedOrderDealCommandKeys.UpdateCommandKeyPrefix, cancellationToken);

			_logger.LogInformation(
				"Успешно обновлено {SuccessfulDealsCount} сделок по плановым заказам из запланированных {PlannedDealsCount}",
				sendResult.SuccessfulCount,
				dealUpdatesToSend.Count);
		}

		private async Task MarkDealsCompleted(
			IReadOnlyList<PlannedOrderDealUpdateDto> succeededDealUpdates,
			CancellationToken cancellationToken)
		{
			var plannedOrderIds = succeededDealUpdates
				.Select(x => x.PlannedOrderId)
				.ToArray();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(PlannedOrdersDealsCreateService)}. Обновление стадии завершённых сделок"))
			{
				var plannedOrders = _plannedOrderRepository
					.Get(uow, x => plannedOrderIds.Contains(x.Id));

				var now = DateTime.UtcNow.ToMoscowDateTime();

				foreach(var plannedOrder in plannedOrders)
				{
					plannedOrder.Stage = PlannedOrderBitrixDealStage.DealCompleted;
					plannedOrder.LastUpdateDate = now;
					plannedOrder.LastSynchronizedDate = now;
					plannedOrder.LastError = null;

					await uow.SaveAsync(plannedOrder, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private async Task MarkPlannedOrdersStage(
			IReadOnlyList<int> plannedOrderIds,
			PlannedOrderBitrixDealStage stage,
			CancellationToken cancellationToken)
		{
			if(!plannedOrderIds.Any())
			{
				return;
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(
				$"Сервис {nameof(PlannedOrdersDealsCreateService)}. Обновление стадии планируемых заказов"))
			{
				var now = DateTime.UtcNow.ToMoscowDateTime();

				foreach(var idsChunk in plannedOrderIds.SplitToChunks(_maxIdsInQuery))
				{
					var ids = idsChunk.ToArray();

					var plannedOrders = _plannedOrderRepository
						.Get(uow, x => ids.Contains(x.Id));

					foreach(var plannedOrder in plannedOrders)
					{
						plannedOrder.Stage = stage;
						plannedOrder.LastUpdateDate = now;

						await uow.SaveAsync(plannedOrder, cancellationToken: cancellationToken);
					}
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private static PlannedOrderDealUpdateDto CreatePlannedOrderDealUpdateDto(PlannedOrder plannedOrder, string stageId) =>
			new PlannedOrderDealUpdateDto
			{
				PlannedOrderId = plannedOrder.Id,
				BitrixDealId = plannedOrder.BitrixDealId.Value,
				StageId = stageId,
				CreatedOrderId = plannedOrder.CreatedOrderId,
				CreatedOrderDeliveryDate = plannedOrder.CreatedOrderDeliveryDate
			};
	}
}
