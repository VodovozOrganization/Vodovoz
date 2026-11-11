using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxOnlineOrderSyncProcessor : IMindboxOnlineOrderSyncProcessor
	{
		private const string SuccessStatus = "Success";
		private const string TransactionAlreadyProcessedStatus = "TransactionAlreadyProcessed";
		private const string PriceHasBeenChangedStatus = "PriceHasBeenChanged";
		private const string CanceledStatus = "Canceled";

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxClient _client;
		private readonly IMindboxAuthorizedLoyaltyOrderService _loyaltyOrderService;
		private readonly IMindboxOnlineOrderStateBuilder _stateBuilder;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxOnlineOrderSyncProcessor> _logger;

		public MindboxOnlineOrderSyncProcessor(
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxClient client,
			IMindboxAuthorizedLoyaltyOrderService loyaltyOrderService,
			IMindboxOnlineOrderStateBuilder stateBuilder,
			IMindboxSettings settings,
			ILogger<MindboxOnlineOrderSyncProcessor> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_loyaltyOrderService = loyaltyOrderService ?? throw new ArgumentNullException(nameof(loyaltyOrderService));
			_stateBuilder = stateBuilder ?? throw new ArgumentNullException(nameof(stateBuilder));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task SyncAsync(int onlineOrderId, Guid transactionId, CancellationToken cancellationToken)
		{
			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Синхронизация онлайн-заказа с Mindbox");
			var onlineOrder = unitOfWork.GetById<OnlineOrder>(onlineOrderId);

			if(onlineOrder is null)
			{
				_logger.LogWarning("Онлайн-заказ {OnlineOrderId} не найден", onlineOrderId);
				return;
			}

			if(transactionId == Guid.Empty)
			{
				transactionId = onlineOrder.MindboxTransactionId ?? Guid.NewGuid();
			}

			var now = DateTime.UtcNow;
			var snapshot = DeserializeSnapshot(onlineOrder.MindboxSnapshot);
			var current = _stateBuilder.BuildState(onlineOrder);
			var isCanceled = onlineOrder.OnlineOrderStatus == OnlineOrderStatus.Canceled
				|| string.Equals(current.Status, CanceledStatus, StringComparison.OrdinalIgnoreCase);

			if(snapshot is null && isCanceled)
			{
				onlineOrder.MindboxSyncRequestedAt = null;
				onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.Unknown;
				onlineOrder.MindboxLastAttemptAt = now;
				onlineOrder.MindboxLastError = "Заказ отменён до регистрации в Mindbox";
				Save(unitOfWork, onlineOrder);
				await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
				return;
			}

			if(snapshot is null)
			{
				await SendCreateAsync(unitOfWork, onlineOrder, current, transactionId, cancellationToken)
					.ConfigureAwait(false);
				return;
			}

			if(isCanceled)
			{
				await SendStatusAsync(
					unitOfWork, onlineOrder, snapshot, current, transactionId, CanceledStatus, true, cancellationToken)
					.ConfigureAwait(false);
				return;
			}

			if(MindboxOnlineOrderStateComparer.HasCompositionChanges(snapshot, current))
			{
				var recalculated = RecalculateBonuses(snapshot, current);
				await SendCreateAsync(unitOfWork, onlineOrder, recalculated, transactionId, cancellationToken)
					.ConfigureAwait(false);
				return;
			}

			if(MindboxOnlineOrderStateComparer.HasStatusChanges(snapshot, current))
			{
				var changedLines = MindboxOnlineOrderStateComparer.GetLinesWithChangedStatus(snapshot, current);
				var updateAll = changedLines.Count == current.Lines.Count;
				await SendStatusAsync(
					unitOfWork, onlineOrder, snapshot, current, transactionId, current.Status, updateAll, cancellationToken)
					.ConfigureAwait(false);
				return;
			}

			onlineOrder.MindboxSyncRequestedAt = null;
			onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.Sent;
			onlineOrder.MindboxLastAttemptAt = now;
			Save(unitOfWork, onlineOrder);
			await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
		}

		private async Task SendCreateAsync(
			IUnitOfWork unitOfWork,
			OnlineOrder onlineOrder,
			MindboxOnlineOrderState state,
			Guid transactionId,
			CancellationToken cancellationToken)
		{
			var counterpartyId = onlineOrder.CounterpartyId ?? onlineOrder.Counterparty?.Id ?? 0;

			if(counterpartyId <= 0)
			{
				await MarkDataErrorAsync(unitOfWork, onlineOrder, "Не найден контрагент", cancellationToken);
				return;
			}

			var result = await _loyaltyOrderService.CreateAsync(
				_stateBuilder.BuildCreateRequest(onlineOrder, state),
				onlineOrder.Id,
				onlineOrder.ExternalOrderId,
				counterpartyId,
				transactionId,
				cancellationToken).ConfigureAwait(false);

			var now = DateTime.UtcNow;

			if(result.Outcome == MindboxLoyaltyOrderOutcome.BonusesChanged)
			{
				await MarkDataErrorAsync(unitOfWork, onlineOrder, result.ErrorMessage, cancellationToken);
				return;
			}

			if(!result.IsSuccess)
			{
				var error = BuildError(result.HttpStatusCode, result.Status, result.ErrorMessage, result.ErrorId);

				if(IsTransient(result.HttpStatusCode))
				{
					await MarkTransientAsync(unitOfWork, onlineOrder, result.HttpStatusCode, result.Status, error, cancellationToken);
					throw new MindboxTransientException(error);
				}

				if(result.HttpStatusCode == 400)
				{
					await MarkDataErrorAsync(unitOfWork, onlineOrder, error, cancellationToken);
				}
				else
				{
					await MarkErrorAsync(unitOfWork, onlineOrder, result.HttpStatusCode, result.Status, error, cancellationToken);
				}

				return;
			}

			onlineOrder.MindboxOrderId = result.MindboxOrderId ?? onlineOrder.MindboxOrderId;
			onlineOrder.BonusesWrittenOff = result.BonusesWrittenOff;
			onlineOrder.BonusesToEarn = result.BonusesToEarn ?? state.BonusesToEarn;
			onlineOrder.BonusesAvailable = false;
			ApplyLineBonuses(onlineOrder, state);

			onlineOrder.MindboxSnapshot = SerializeSnapshot(state);
			onlineOrder.MindboxSnapshotUpdatedAt = now;
			onlineOrder.MindboxLastChangeAt = now;
			onlineOrder.MindboxTransactionId = transactionId;
			onlineOrder.MindboxSyncRequestedAt = null;
			onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.Sent;
			ApplyAttempt(onlineOrder, now, result.HttpStatusCode, result.Status, null, result.ErrorId);
			Save(unitOfWork, onlineOrder);
			await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

			_logger.LogInformation(
				"Онлайн-заказ сохранён в Mindbox: onlineOrderId={OnlineOrderId}, transactionId={TransactionId}, mindboxId={MindboxId}, bonusesWrittenOff={BonusesWrittenOff}, bonusesToEarn={BonusesToEarn}, httpCode={HttpCode}, status={Status}",
				onlineOrder.Id,
				transactionId,
				onlineOrder.MindboxOrderId,
				onlineOrder.BonusesWrittenOff,
				onlineOrder.BonusesToEarn,
				result.HttpStatusCode,
				result.Status);
		}

		private async Task SendStatusAsync(
			IUnitOfWork unitOfWork,
			OnlineOrder onlineOrder,
			MindboxOnlineOrderState snapshot,
			MindboxOnlineOrderState current,
			Guid transactionId,
			string newStatus,
			bool updateAll,
			CancellationToken cancellationToken)
		{
			var changedLines = updateAll
				? current.Lines
				: MindboxOnlineOrderStateComparer.GetLinesWithChangedStatus(snapshot, current);

			if(!updateAll && changedLines.Count == 0)
			{
				onlineOrder.MindboxSyncRequestedAt = null;
				onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.Sent;
				Save(unitOfWork, onlineOrder);
				await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
				return;
			}

			var request = _stateBuilder.BuildStatusRequest(
				onlineOrder, current, changedLines, newStatus, updateAll);
			request.ExecutionDateTimeUtc = DateTime.SpecifyKind(
				onlineOrder.MindboxLastChangeAt ?? DateTime.UtcNow,
				DateTimeKind.Utc);

			var result = await _client.UpdateOrderLinesStatusAsync(request, transactionId, cancellationToken)
				.ConfigureAwait(false);

			var now = DateTime.UtcNow;

			if(IsSuccess(result))
			{
				ApplyStatuses(snapshot, changedLines, newStatus, updateAll);
				snapshot.Status = newStatus;
				snapshot.TotalPrice = current.TotalPrice;
				snapshot.DeliveryCost = current.DeliveryCost;

				if(string.Equals(newStatus, CanceledStatus, StringComparison.OrdinalIgnoreCase))
				{
					snapshot.BonusesWrittenOff = 0;
					snapshot.BonusesToEarn = 0;
					onlineOrder.BonusesWrittenOff = 0;
					onlineOrder.BonusesToEarn = 0;
					onlineOrder.BonusesAvailable = false;

					foreach(var item in onlineOrder.OnlineOrderItems)
					{
						item.BonusesWrittenOff = 0;
					}

					foreach(var line in snapshot.Lines)
					{
						line.BonusesWrittenOff = 0;
					}
				}

				onlineOrder.MindboxSnapshot = SerializeSnapshot(snapshot);
				onlineOrder.MindboxSnapshotUpdatedAt = now;
				onlineOrder.MindboxLastChangeAt = now;
				onlineOrder.MindboxTransactionId = transactionId;
				onlineOrder.MindboxSyncRequestedAt = null;
				onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.Sent;
				ApplyAttempt(onlineOrder, now, result.HttpStatusCode, result.Status, null, result.ErrorId);
				Save(unitOfWork, onlineOrder);
				await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

				_logger.LogInformation(
					"Статус позиций онлайн-заказа изменён в Mindbox: onlineOrderId={OnlineOrderId}, transactionId={TransactionId}, status={Status}, httpCode={HttpCode}, mindboxStatus={Status}",
					onlineOrder.Id, transactionId, newStatus, result.HttpStatusCode, result.Status);
				return;
			}

			var error = BuildError(result.HttpStatusCode, result.Status, result.ErrorMessage, result.ErrorId);

			if(string.Equals(result.Status, PriceHasBeenChangedStatus, StringComparison.OrdinalIgnoreCase))
			{
				await MarkDataErrorAsync(unitOfWork, onlineOrder, error, cancellationToken);
				return;
			}

			if(IsTransient(result.HttpStatusCode))
			{
				await MarkTransientAsync(unitOfWork, onlineOrder, result.HttpStatusCode, result.Status, error, cancellationToken);
				throw new MindboxTransientException(error);
			}

			await MarkErrorAsync(unitOfWork, onlineOrder, result.HttpStatusCode, result.Status, error, cancellationToken);
		}

		private static MindboxOnlineOrderState RecalculateBonuses(
			MindboxOnlineOrderState snapshot,
			MindboxOnlineOrderState current)
		{
			var snapshotLines = (snapshot.Lines ?? new List<MindboxOnlineOrderStateLine>())
				.ToDictionary(line => line.LineId, StringComparer.Ordinal);

			foreach(var currentLine in current.Lines)
			{
				if(snapshotLines.TryGetValue(currentLine.LineId, out var snapshotLine) && snapshotLine.Quantity > 0)
				{
					var perUnit = (decimal)snapshotLine.BonusesWrittenOff / snapshotLine.Quantity;
					currentLine.BonusesWrittenOff = (int)Math.Round(perUnit * currentLine.Quantity, MidpointRounding.AwayFromZero);
				}
				else
				{
					currentLine.BonusesWrittenOff = 0;
				}
			}

			current.BonusesWrittenOff = current.Lines.Sum(line => line.BonusesWrittenOff);
			return current;
		}

		private static void ApplyLineBonuses(OnlineOrder onlineOrder, MindboxOnlineOrderState state)
		{
			var lines = (state.Lines ?? new List<MindboxOnlineOrderStateLine>())
				.ToDictionary(line => line.LineId, StringComparer.Ordinal);

			foreach(var item in onlineOrder.OnlineOrderItems)
			{
				if(lines.TryGetValue(item.Id.ToString(), out var line))
				{
					item.BonusesWrittenOff = line.BonusesWrittenOff;
				}
			}
		}

		private static void ApplyStatuses(
			MindboxOnlineOrderState snapshot,
			IReadOnlyCollection<MindboxOnlineOrderStateLine> changedLines,
			string newStatus,
			bool updateAll)
		{
			if(snapshot.Lines is null)
			{
				return;
			}

			if(updateAll)
			{
				foreach(var line in snapshot.Lines)
				{
					line.Status = newStatus;
				}

				return;
			}

			var ids = new HashSet<string>(changedLines.Select(line => line.LineId), StringComparer.Ordinal);

			foreach(var line in snapshot.Lines.Where(line => ids.Contains(line.LineId)))
			{
				line.Status = newStatus;
			}
		}

		private static bool IsSuccess(MindboxOperationResult result)
		{
			return result.IsSuccess
				|| string.Equals(result.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(result.Status, TransactionAlreadyProcessedStatus, StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsTransient(int httpStatusCode)
		{
			return httpStatusCode == 0 || httpStatusCode == 401 || httpStatusCode == 403
				|| httpStatusCode == 429 || httpStatusCode >= 500;
		}

		private async Task MarkTransientAsync(
			IUnitOfWork unitOfWork, OnlineOrder onlineOrder, int httpStatusCode, string status,
			string error, CancellationToken cancellationToken)
		{
			var now = DateTime.UtcNow;
			onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.Queued;
			onlineOrder.MindboxSyncRequestedAt = now;
			ApplyAttempt(onlineOrder, now, httpStatusCode, status, error, onlineOrder.MindboxLastErrorId);
			Save(unitOfWork, onlineOrder);
			await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
			_logger.LogWarning("Временная ошибка Mindbox: onlineOrderId={OnlineOrderId}, error={Error}", onlineOrder.Id, error);
		}

		private async Task MarkDataErrorAsync(
			IUnitOfWork unitOfWork, OnlineOrder onlineOrder, string error, CancellationToken cancellationToken)
		{
			var now = DateTime.UtcNow;
			onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.DataError;
			onlineOrder.MindboxSyncRequestedAt = null;
			ApplyAttempt(onlineOrder, now, onlineOrder.MindboxLastHttpStatusCode ?? 0, null, error, null);
			Save(unitOfWork, onlineOrder);
			await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
			_logger.LogError("Ошибка данных Mindbox: onlineOrderId={OnlineOrderId}, error={Error}", onlineOrder.Id, error);
		}

		private async Task MarkErrorAsync(
			IUnitOfWork unitOfWork, OnlineOrder onlineOrder, int httpStatusCode, string status,
			string error, CancellationToken cancellationToken)
		{
			var now = DateTime.UtcNow;
			onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.Error;
			onlineOrder.MindboxSyncRequestedAt = null;
			ApplyAttempt(onlineOrder, now, httpStatusCode, status, error, null);
			Save(unitOfWork, onlineOrder);
			await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
			_logger.LogError("Ошибка синхронизации Mindbox: onlineOrderId={OnlineOrderId}, error={Error}", onlineOrder.Id, error);
		}

		private static void ApplyAttempt(
			OnlineOrder onlineOrder, DateTime now, int httpStatusCode, string status, string error, string errorId)
		{
			onlineOrder.MindboxLastAttemptAt = now;
			onlineOrder.MindboxAttempts++;
			onlineOrder.MindboxLastHttpStatusCode = httpStatusCode;
			onlineOrder.MindboxLastStatus = status;
			onlineOrder.MindboxLastError = Truncate(error, 2000);
			onlineOrder.MindboxLastErrorId = errorId;
		}

		private static string BuildError(int httpCode, string status, string error, string errorId)
		{
			var parts = new List<string>();
			if(httpCode != 0) parts.Add($"httpCode={httpCode}");
			if(!string.IsNullOrWhiteSpace(status)) parts.Add($"status={status}");
			if(!string.IsNullOrWhiteSpace(error)) parts.Add(error);
			if(!string.IsNullOrWhiteSpace(errorId)) parts.Add($"errorId={errorId}");
			return Truncate(string.Join("; ", parts), 2000);
		}

		private static string Truncate(string value, int max)
		{
			return string.IsNullOrEmpty(value) || value.Length <= max ? value : value.Substring(0, max);
		}

		private static MindboxOnlineOrderState DeserializeSnapshot(string snapshot)
		{
			if(string.IsNullOrWhiteSpace(snapshot)) return null;

			try
			{
				return JsonSerializer.Deserialize<MindboxOnlineOrderState>(snapshot);
			}
			catch(JsonException)
			{
				return null;
			}
		}

		private static string SerializeSnapshot(MindboxOnlineOrderState state)
		{
			return JsonSerializer.Serialize(state);
		}

		private static void Save(IUnitOfWork unitOfWork, OnlineOrder onlineOrder)
		{
			unitOfWork.Save(onlineOrder);
		}
	}
}
