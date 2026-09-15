using DatabaseServiceWorker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using QS.Project.DB;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Infrastructure;

namespace DatabaseServiceWorker
{
	/// <summary>
	/// Периодически пересчитывает частоту заказов без участия в сохранении заказов.
	/// </summary>
	internal sealed class DeliveryPointOrderFrequencyWorker : TimerBackgroundServiceBase
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<DeliveryPointOrderFrequencyWorker> _logger;
		private readonly DeliveryPointOrderFrequencyOptions _options;

		/// <summary>
		/// Создаёт обработчик фонового пересчёта частоты заказов.
		/// </summary>
		/// <param name="scopeFactory">Фабрика областей зависимостей.</param>
		/// <param name="logger">Журнал событий обработчика.</param>
		/// <param name="options">Настройки периодичности и размера порций.</param>
		public DeliveryPointOrderFrequencyWorker(
			IServiceScopeFactory scopeFactory,
			ILogger<DeliveryPointOrderFrequencyWorker> logger,
			IOptions<DeliveryPointOrderFrequencyOptions> options)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
			if(_options.Interval <= TimeSpan.Zero || _options.BatchSize <= 0 || _options.BatchDelay < TimeSpan.Zero)
			{
				throw new ArgumentException("Интервал и размер порции должны быть положительными, пауза между порциями — неотрицательной.", nameof(options));
			}
		}

		protected override TimeSpan Interval => _options.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			// Синхронные запросы репозитория не должны задерживать запуск остальных сервисов хоста.
			await Task.Yield();
			var stopwatch = Stopwatch.StartNew();
			var processed = 0;
			var failed = 0;
			var skipped = 0;
			try
			{
				using(var scope = _scopeFactory.CreateScope())
				{
					var uowFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
					var repository = scope.ServiceProvider.GetRequiredService<IDeliveryPointRepository>();
					IDatabase cache = null;
					string cacheKeyPrefix = null;
					try
					{
						var databaseName = scope.ServiceProvider.GetRequiredService<IDatabaseConnectionSettings>().DatabaseName;
						cache = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
						cacheKeyPrefix = $"order-frequency:v2:{databaseName}:";
					}
					catch(Exception exception)
					{
						_logger.LogWarning("Кэш частоты заказов недоступен ({ErrorType}); проход будет выполнен без кэша", exception.GetType().Name);
					}
					var afterId = 0;
					while(true)
					{
						stoppingToken.ThrowIfCancellationRequested();
						IList<int> ids;
						using(var uow = uowFactory.CreateWithoutRoot(nameof(DeliveryPointOrderFrequencyWorker)))
						{
							ids = repository.GetDeliveryPointIdsBatch(uow, afterId, _options.BatchSize);
						}

						if(ids.Count == 0)
						{
							break;
						}

						foreach(var id in ids)
						{
							stoppingToken.ThrowIfCancellationRequested();
							DeliveryPointOrderFrequencyCacheEntry cached = null;
							if(cache != null)
							{
								try
								{
									var value = await cache.StringGetAsync(cacheKeyPrefix + id);
									cached = value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<DeliveryPointOrderFrequencyCacheEntry>((string)value);
								}
								catch(Exception exception)
								{
									_logger.LogWarning("Ошибка чтения кэша частоты заказов ({ErrorType}); продолжим проход без кэша", exception.GetType().Name);
									cache = null;
								}
							}
							try
							{
								DeliveryPointOrderFrequencyCacheEntry entry;
								// Новая сессия и короткая транзакция для каждой точки: сбой не затрагивает другие точки.
								using(var uow = uowFactory.CreateWithoutRoot(nameof(DeliveryPointOrderFrequencyWorker)))
								using(var transaction = uow.Session.BeginTransaction())
								{
									var state = cache == null ? null : repository.GetOrderFrequencyState(uow, id);
									if(cached?.State != null && state != null
										&& cached.State.Select(order => (order.OrderId, order.DeliveryDate))
											.SequenceEqual(state.Select(order => (order.OrderId, order.DeliveryDate))))
									{
										skipped++;
										afterId = id;
										continue;
									}

									entry = new DeliveryPointOrderFrequencyCacheEntry
									{
										State = state,
										Frequency = repository.UpdateOrderFrequency(uow, id)
									};
									stoppingToken.ThrowIfCancellationRequested();
									transaction.Commit();
								}
								processed++;
								// Запись в кэш только после успешного commit; хранение без срока истечения.
								if(cache != null)
								{
									try
									{
										await cache.StringSetAsync(cacheKeyPrefix + id, JsonSerializer.Serialize(entry));
									}
									catch(Exception exception)
									{
										_logger.LogWarning("Ошибка записи кэша частоты заказов ({ErrorType}); результат сохранён в БД, продолжим проход без кэша", exception.GetType().Name);
										cache = null;
									}
								}
							}
							catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
							{
								throw;
							}
							catch(Exception exception)
							{
								failed++;
								_logger.LogError(exception, "Не удалось пересчитать частоту заказов точки {DeliveryPointId}; повторим в следующем проходе", id);
							}
							// Ошибочную точку повторно обработает следующий полный проход.
							afterId = id;
						}

						await Task.Delay(_options.BatchDelay, stoppingToken);
					}
				}
				stopwatch.Stop();
				_logger.LogInformation("Пересчёт частоты заказов завершён: обработано {ProcessedCount} точек, пропущено по кэшу {SkippedCount}, ошибок {ErrorCount}, время {ElapsedMilliseconds} мс",
					processed, skipped, failed, stopwatch.ElapsedMilliseconds);
			}
			catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
			{
				// Штатная остановка; следующий запуск начнёт полный проход заново.
			}
			catch(Exception exception)
			{
				_logger.LogError(exception, "Ошибка прохода пересчёта частоты заказов; следующий проход начнётся через {Interval}", Interval);
			}
		}
	}
}
