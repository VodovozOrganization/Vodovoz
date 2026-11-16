using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxBonusService : IMindboxBonusService
	{
		private const string CustomerIdName = "mindboxId";
		private const string CustomerIdValue = "11";
		private const string CalculatedStatus = "Calculated";

		private readonly IMindboxClient _client;
		private readonly IMindboxSettings _settings;
		private readonly IMindboxBonusCircuitBreaker _circuitBreaker;
		private readonly IMindboxAlertSender _alertSender;
		private readonly IMemoryCache _cache;
		private readonly ILogger<MindboxBonusService> _logger;

		public MindboxBonusService(
			IMindboxClient client,
			IMindboxSettings settings,
			IMindboxBonusCircuitBreaker circuitBreaker,
			IMindboxAlertSender alertSender,
			IMemoryCache cache,
			ILogger<MindboxBonusService> logger)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
			_alertSender = alertSender ?? throw new ArgumentNullException(nameof(alertSender));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<IReadOnlyDictionary<string, int?>> CalculateBonusesAsync(
			IReadOnlyCollection<MindboxBonusRequestItem> items,
			CancellationToken cancellationToken)
		{
			var bonuses = new ConcurrentDictionary<string, int?>(StringComparer.Ordinal);

			if(items is null || items.Count == 0)
			{
				return bonuses;
			}

			if(!_settings.BonusCalculationEnabled)
			{
				_logger.LogDebug("Расчёт бонусов Mindbox выключен настройкой");

				return bonuses;
			}

			if(_circuitBreaker.IsPaused(DateTime.UtcNow))
			{
				_logger.LogInformation(
					"Расчёт бонусов Mindbox на паузе до {PausedUntil}",
					_circuitBreaker.PausedUntil);

				return bonuses;
			}

			var cacheSeconds = _settings.BonusCacheSeconds;
			var pending = new List<MindboxBonusRequestItem>();

			foreach(var item in Deduplicate(items))
			{
				if(TryGetCached(item, cacheSeconds, out var cachedBonus))
				{
					bonuses[item.ItemKey] = cachedBonus;
				}
				else
				{
					pending.Add(item);
				}
			}

			if(pending.Count == 0)
			{
				return bonuses;
			}

			var batchSize = Math.Max(1, _settings.BonusBatchSize);
			var maxParallelBatches = Math.Max(1, _settings.BonusMaxParallelBatches);
			var batchTimeout = TimeSpan.FromSeconds(Math.Max(1, _settings.BonusBatchTimeoutSeconds));
			var totalTimeout = TimeSpan.FromMilliseconds(Math.Max(1, _settings.BonusTotalTimeoutMilliseconds));

			var batches = Chunk(pending, batchSize).ToList();
			var successfulBatches = 0;
			var stopwatch = Stopwatch.StartNew();

			using(var totalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
			using(var semaphore = new SemaphoreSlim(maxParallelBatches, maxParallelBatches))
			{
				totalCts.CancelAfter(totalTimeout);

				var tasks = batches
					.Select(batch => ProcessBatchAsync(
						batch,
						semaphore,
						totalCts.Token,
						cancellationToken,
						batchTimeout,
						cacheSeconds,
						bonuses))
					.ToList();

				try
				{
					var results = await Task.WhenAll(tasks);
					successfulBatches = results.Count(x => x);
				}
				catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
				{
					throw;
				}
				catch(OperationCanceledException)
				{
					_logger.LogWarning(
						"Расчёт бонусов Mindbox прерван по общему лимиту {TotalTimeoutMs}мс",
						totalTimeout.TotalMilliseconds);
				}
			}

			stopwatch.Stop();

			_logger.LogInformation(
				"Расчёт бонусов Mindbox: позиций={ItemsCount}, с бонусами={BonusesCount}, пачек={BatchesCount}, успешных пачек={SuccessfulBatches}, ожидание={DurationMs}мс",
				items.Count,
				bonuses.Count,
				batches.Count,
				successfulBatches,
				stopwatch.ElapsedMilliseconds);

			return bonuses;
		}

		private async Task<bool> ProcessBatchAsync(
			IReadOnlyList<MindboxBonusRequestItem> batch,
			SemaphoreSlim semaphore,
			CancellationToken totalToken,
			CancellationToken callerToken,
			TimeSpan batchTimeout,
			int cacheSeconds,
			ConcurrentDictionary<string, int?> bonuses)
		{
			var acquired = false;
			var callStarted = false;

			try
			{
				await semaphore.WaitAsync(totalToken);
				acquired = true;

				if(!_circuitBreaker.TryBeginCall(DateTime.UtcNow))
				{
					_logger.LogInformation("Расчёт бонусов Mindbox: пачка пропущена из-за паузы");

					return false;
				}

				callStarted = true;

				using(var batchCts = CancellationTokenSource.CreateLinkedTokenSource(totalToken))
				{
					batchCts.CancelAfter(batchTimeout);

					var request = BuildRequest(batch);
					var operation = await _client.CalculateProductListAsync(request, batchCts.Token);

					if(operation.IsSuccess)
					{
						var batchBonuses = ParseResponse(operation.RawResponse, batch);

						_circuitBreaker.RegisterSuccess();
						StoreCached(batch, batchBonuses, cacheSeconds);

						foreach(var bonus in batchBonuses)
						{
							bonuses[bonus.Key] = bonus.Value;
						}

						return true;
					}

					await HandleFailureAsync(operation);
				}
			}
			catch(OperationCanceledException) when(callerToken.IsCancellationRequested)
			{
				throw;
			}
			catch(OperationCanceledException)
			{
				_logger.LogWarning(
					"Расчёт бонусов Mindbox: пачка не ответила за {BatchTimeoutSeconds}с",
					batchTimeout.TotalSeconds);

				if(callStarted)
				{
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, true, "Timeout");
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Расчёт бонусов Mindbox: ошибка обработки пачки");
			}
			finally
			{
				if(acquired)
				{
					semaphore.Release();
				}
			}

			return false;
		}

		private async Task HandleFailureAsync(MindboxOperationResult operation)
		{
			var details = string.Format(
				CultureInfo.InvariantCulture,
				"HTTP={0}, status={1}, errorId={2}, длительность={3}мс",
				operation.HttpStatusCode,
				operation.Status,
				operation.ErrorId,
				operation.DurationMs);

			switch(operation.Outcome)
			{
				case MindboxSendOutcome.ValidationError:
					_logger.LogError(
						"Расчёт бонусов Mindbox: ошибка в данных ERP. {Details}",
						details);

					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
				case MindboxSendOutcome.ProtocolError:
					_logger.LogError(
						"Расчёт бонусов Mindbox: ошибка запроса. {Details}",
						details);

					await SendAlertAsync("Mindbox CalculateProductList: ошибка запроса (400)");
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
				case MindboxSendOutcome.NotFound:
					_logger.LogError(
						"Расчёт бонусов Mindbox: операция не найдена. {Details}",
						details);

					await SendAlertAsync("Mindbox CalculateProductList: операция не найдена (404)");
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
				case MindboxSendOutcome.Unauthorized:
					_logger.LogError(
						"Расчёт бонусов Mindbox: неверный секретный ключ или нет доступа. {Details}",
						details);

					await SendAlertAsync("Mindbox CalculateProductList: неверный секретный ключ (401/403)");
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
				case MindboxSendOutcome.RateLimited:
					_logger.LogError(
						"Расчёт бонусов Mindbox: превышен лимит вызовов. {Details}",
						details);

					await SendAlertAsync("Mindbox CalculateProductList: превышен лимит вызовов (429)");
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, true, "RateLimited");
					break;
				case MindboxSendOutcome.TransientError:
					_logger.LogWarning(
						"Расчёт бонусов Mindbox: временная ошибка. {Details}",
						details);

					_circuitBreaker.RegisterFailure(DateTime.UtcNow, true, "TransientError");
					break;
				default:
					_logger.LogError(
						"Расчёт бонусов Mindbox: неизвестная ошибка. {Details}",
						details);

					break;
			}
		}

		private async Task SendAlertAsync(string message)
		{
			try
			{
				await _alertSender.AlertAsync(message, CancellationToken.None);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось отправить алерт Mindbox: {Message}", message);
			}
		}

		private MindboxCalculateProductListRequest BuildRequest(IReadOnlyList<MindboxBonusRequestItem> batch)
		{
			var itemIdName = _settings.BonusItemIdName;

			var requestItems = new List<MindboxCalculateProductListItem>(batch.Count);

			foreach(var item in batch)
			{
				requestItems.Add(new MindboxCalculateProductListItem
				{
					BasePricePerItem = item.BasePrice,
					Product = new MindboxProduct
					{
						Ids = new Dictionary<string, string>(StringComparer.Ordinal)
						{
							[itemIdName] = item.ItemKey
						}
					}
				});
			}

			return new MindboxCalculateProductListRequest
			{
				Customer = new MindboxCalculateProductListCustomer
				{
					Ids = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						[CustomerIdName] = CustomerIdValue
					}
				},
				ProductList = new MindboxCalculateProductList
				{
					Items = requestItems
				}
			};
		}

		private IReadOnlyDictionary<string, int?> ParseResponse(
			string rawResponse,
			IReadOnlyList<MindboxBonusRequestItem> batch)
		{
			var result = new Dictionary<string, int?>(StringComparer.Ordinal);

			if(string.IsNullOrWhiteSpace(rawResponse))
			{
				_logger.LogWarning("Расчёт бонусов Mindbox: пустой ответ");

				return result;
			}

			MindboxCalculateProductListResponse response;

			try
			{
				response = JsonSerializer.Deserialize<MindboxCalculateProductListResponse>(rawResponse);
			}
			catch(JsonException ex)
			{
				_logger.LogWarning(ex, "Расчёт бонусов Mindbox: не удалось разобрать ответ");

				return result;
			}

			if(response?.ProductList is null)
			{
				_logger.LogWarning("Расчёт бонусов Mindbox: в ответе нет productList");

				return result;
			}

			if(!string.Equals(response.ProductList.ProcessingStatus, CalculatedStatus, StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogWarning(
					"Расчёт бонусов Mindbox: processingStatus={ProcessingStatus}",
					response.ProductList.ProcessingStatus);

				return result;
			}

			var requestedKeys = new HashSet<string>(
				batch.Select(x => x.ItemKey),
				StringComparer.Ordinal);
			var itemIdName = _settings.BonusItemIdName;
			var balanceSystemName = _settings.BonusBalanceSystemName;

			if(response.ProductList.Items is null)
			{
				return result;
			}

			foreach(var item in response.ProductList.Items)
			{
				if(item?.Product?.Ids is null
					|| !item.Product.Ids.TryGetValue(itemIdName, out var itemKey)
					|| string.IsNullOrWhiteSpace(itemKey)
					|| !requestedKeys.Contains(itemKey))
				{
					continue;
				}

				result[itemKey] = GetBonusAmount(item.EarnBonusPoints, balanceSystemName);
			}

			return result;
		}

		private static int GetBonusAmount(
			IEnumerable<MindboxEarnBonusPoints> earnBonusPoints,
			string balanceSystemName)
		{
			if(earnBonusPoints is null || string.IsNullOrWhiteSpace(balanceSystemName))
			{
				return 0;
			}

			foreach(var points in earnBonusPoints)
			{
				if(points?.Balance is null)
				{
					continue;
				}

				if(string.Equals(points.Balance.SystemName, balanceSystemName, StringComparison.Ordinal))
				{
					return (int)Math.Floor(points.Amount);
				}
			}

			return 0;
		}

		private bool TryGetCached(MindboxBonusRequestItem item, int cacheSeconds, out int? bonus)
		{
			bonus = null;

			if(cacheSeconds <= 0)
			{
				return false;
			}

			return _cache.TryGetValue(BuildCacheKey(item), out bonus);
		}

		private void StoreCached(
			IReadOnlyList<MindboxBonusRequestItem> batch,
			IReadOnlyDictionary<string, int?> bonuses,
			int cacheSeconds)
		{
			if(cacheSeconds <= 0)
			{
				return;
			}

			var options = new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheSeconds)
			};

			foreach(var item in batch)
			{
				if(bonuses.TryGetValue(item.ItemKey, out var bonus))
				{
					_cache.Set(BuildCacheKey(item), bonus, options);
				}
			}
		}

		private static string BuildCacheKey(MindboxBonusRequestItem item)
		{
			return string.Concat(
				"mindbox:bonus:",
				item.ItemKey,
				":",
				item.BasePrice.ToString(CultureInfo.InvariantCulture));
		}

		private static IEnumerable<MindboxBonusRequestItem> Deduplicate(
			IEnumerable<MindboxBonusRequestItem> items)
		{
			var seen = new HashSet<string>(StringComparer.Ordinal);

			foreach(var item in items)
			{
				if(seen.Add(item.ItemKey))
				{
					yield return item;
				}
			}
		}

		private static IEnumerable<IReadOnlyList<MindboxBonusRequestItem>> Chunk(
			IReadOnlyList<MindboxBonusRequestItem> items,
			int batchSize)
		{
			for(var offset = 0; offset < items.Count; offset += batchSize)
			{
				var size = Math.Min(batchSize, items.Count - offset);
				var chunk = new MindboxBonusRequestItem[size];

				for(var index = 0; index < size; index++)
				{
					chunk[index] = items[offset + index];
				}

				yield return chunk;
			}
		}
	}
}
