using BitrixNotificationsSend.Contracts;
using BitrixNotificationsSend.Contracts.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace BitrixNotificationsSend.Library.Services.Batches
{
	public partial class BitrixBatchesSendService : IBitrixBatchesSendService
	{
		/// <summary>
		/// Пауза между пакетными запросами, чтобы не превышать ограничение Битрикс24 на интенсивность запросов
		/// </summary>
		private static readonly TimeSpan _delayBetweenBatches = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Порог накопленного операционного времени метода, сек, после которого делается пауза до сброса бюджета
		/// Лимит Битрикс24 - <see cref="BitrixApiLimits.OperatingLimitSeconds"/> сек на метод в 10-минутном окне,
		/// запас нужен, чтобы очередной пакет команд не упёрся в лимит
		/// </summary>
		private const double _operatingSecondsThreshold = 400;

		/// <summary>
		/// Буфер к времени ожидания сброса операционного бюджета Битрикс24
		/// </summary>
		private static readonly TimeSpan _operatingResetBuffer = TimeSpan.FromSeconds(5);

		private readonly ILogger<BitrixBatchesSendService> _logger;

		public BitrixBatchesSendService(ILogger<BitrixBatchesSendService> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<BatchesSendResult<TItem>> SendAll<TItem>(
			IReadOnlyList<TItem> items,
			Func<TItem, string> commandKeySelector,
			Func<IReadOnlyList<TItem>, CancellationToken, Task<Result<BitrixBatchResult>>> sendBatch,
			Func<IReadOnlyList<TItem>, CancellationToken, Task> onBatchItemsSucceeded,
			CancellationToken cancellationToken)
		{
			if(items is null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			if(commandKeySelector is null)
			{
				throw new ArgumentNullException(nameof(commandKeySelector));
			}

			if(sendBatch is null)
			{
				throw new ArgumentNullException(nameof(sendBatch));
			}

			if(onBatchItemsSucceeded is null)
			{
				throw new ArgumentNullException(nameof(onBatchItemsSucceeded));
			}

			var sendResult = await SendBatchesSeries(items, commandKeySelector, sendBatch, onBatchItemsSucceeded, cancellationToken);

			if(!sendResult.OperatingLimitFailedItems.Any())
			{
				return sendResult;
			}

			_logger.LogInformation(
				"{OperatingLimitFailedCount} команд не выполнено из-за операционного лимита Битрикс24. " +
				"Ожидаем освобождения бюджета и отправляем их повторно",
				sendResult.OperatingLimitFailedItems.Count);

			await WaitForOperatingReset(sendResult.OperatingResetAt, cancellationToken);

			var retrySendResult = await SendBatchesSeries(
				sendResult.OperatingLimitFailedItems,
				commandKeySelector,
				sendBatch,
				onBatchItemsSucceeded,
				cancellationToken);

			retrySendResult.SuccessfulCount += sendResult.SuccessfulCount;

			if(retrySendResult.OperatingLimitFailedItems.Any())
			{
				_logger.LogError(
					"После повторной отправки не выполнено {OperatingLimitFailedCount} команд " +
					"из-за операционного лимита Битрикс24",
					retrySendResult.OperatingLimitFailedItems.Count);
			}

			return retrySendResult;
		}

		private async Task<BatchesSeriesSendResult<TItem>> SendBatchesSeries<TItem>(
			IReadOnlyList<TItem> items,
			Func<TItem, string> commandKeySelector,
			Func<IReadOnlyList<TItem>, CancellationToken, Task<Result<BitrixBatchResult>>> sendBatch,
			Func<IReadOnlyList<TItem>, CancellationToken, Task> onBatchItemsSucceeded,
			CancellationToken cancellationToken)
		{
			var seriesResult = new BatchesSeriesSendResult<TItem>();

			var batchSize = BitrixApiLimits.MaxBatchCommandsCount;
			var batchesCount = (items.Count + batchSize - 1) / batchSize;

			for(var batchIndex = 0; batchIndex < batchesCount; batchIndex++)
			{
				if(batchIndex > 0)
				{
					if(seriesResult.OperatingSeconds >= _operatingSecondsThreshold)
					{
						_logger.LogInformation(
							"Операционное время метода достигло {OperatingSeconds:F0} сек, " +
							"приближаемся к лимиту Битрикс24",
							seriesResult.OperatingSeconds);

						await WaitForOperatingReset(seriesResult.OperatingResetAt, cancellationToken);

						seriesResult.OperatingSeconds = 0;
						seriesResult.OperatingResetAt = null;
					}
					else
					{
						await Task.Delay(_delayBetweenBatches, cancellationToken);
					}
				}

				var batchItems = items
					.Skip(batchIndex * batchSize)
					.Take(batchSize)
					.ToList();

				_logger.LogInformation(
					"Отправляем пакет {BatchNumber} из {BatchesCount} команд в Битрикс24. " +
					"Команд в пакете: {BatchCommandsCount}",
					batchIndex + 1,
					batchesCount,
					batchItems.Count);

				try
				{
					var batchResult = await sendBatch(batchItems, cancellationToken);

					if(batchResult.IsFailure)
					{
						var message = batchResult.Errors.FirstOrDefault()?.Message;
						_logger.LogError(
							"Ошибка отправки пакета команд в Битрикс24: {ErrorMessage}",
							message);
						continue;
					}

					seriesResult.SuccessfulCount += batchResult.Value.SuccessfulCommandKeys.Count;

					var batchItemsByCommandKeys = batchItems.ToDictionary(commandKeySelector);

					var succeededItems = new List<TItem>();

					foreach(var successfulCommandKey in batchResult.Value.SuccessfulCommandKeys)
					{
						if(batchItemsByCommandKeys.TryGetValue(successfulCommandKey, out var succeededItem))
						{
							succeededItems.Add(succeededItem);
						}
					}

					if(succeededItems.Any())
					{
						await onBatchItemsSucceeded(succeededItems, cancellationToken);
					}

					if(batchResult.Value.OperatingSeconds > seriesResult.OperatingSeconds)
					{
						seriesResult.OperatingSeconds = batchResult.Value.OperatingSeconds;
					}

					if(batchResult.Value.OperatingResetAt != null
						&& (seriesResult.OperatingResetAt == null || batchResult.Value.OperatingResetAt > seriesResult.OperatingResetAt))
					{
						seriesResult.OperatingResetAt = batchResult.Value.OperatingResetAt;
					}

					foreach(var itemError in batchResult.Value.Errors)
					{
						if(itemError.IsOperatingLimitError
							&& batchItemsByCommandKeys.TryGetValue(itemError.CommandKey, out var failedItem))
						{
							_logger.LogWarning(
								"Команда {CommandKey} не выполнена из-за операционного лимита Битрикс24",
								itemError.CommandKey);

							seriesResult.OperatingLimitFailedItems.Add(failedItem);
							continue;
						}

						_logger.LogError(
							"Ошибка выполнения команды {CommandKey} в Битрикс24: {ErrorMessage}",
							itemError.CommandKey,
							itemError.Message);
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка отправки пакета команд в Битрикс24");
				}
			}

			return seriesResult;
		}

		private async Task WaitForOperatingReset(DateTime? operatingResetAtUtc, CancellationToken cancellationToken)
		{
			var delay = operatingResetAtUtc.HasValue
				? operatingResetAtUtc.Value - DateTime.UtcNow + _operatingResetBuffer
				: BitrixApiLimits.OperatingWindow;

			var maxDelay = BitrixApiLimits.OperatingWindow + _operatingResetBuffer;

			if(delay > maxDelay)
			{
				delay = maxDelay;
			}

			if(delay <= TimeSpan.Zero)
			{
				return;
			}

			_logger.LogInformation(
				"Ожидаем освобождения операционного бюджета Битрикс24: {DelaySeconds:F0} сек",
				delay.TotalSeconds);

			await Task.Delay(delay, cancellationToken);
		}
	}
}
