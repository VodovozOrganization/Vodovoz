using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services.ReceiptFailedProblem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace Edo.Problem.Routine.Worker
{
	/// <summary>
	/// Фоновый сервис плановой проверки и переотправки чеков с ошибками фискализации (Failed)
	/// </summary>
	public class ReceiptFailedReprocessingWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ReceiptFailedReprocessingWorker> _logger;
		private readonly IOptionsMonitor<ReceiptFailedReprocessingOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IReceiptReprocessingLockCoordinator _lockCoordinator;

		/// <summary>
		/// Конструктор фонового сервиса переобработки чеков с ошибками
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="options">Монитор настроек переобработки</param>
		/// <param name="serviceScopeFactory">Фабрика областей видимости служб внедрения зависимостей</param>
		/// <param name="lockCoordinator">Координатор блокировки параллельного выполнения</param>
		public ReceiptFailedReprocessingWorker(
			ILogger<ReceiptFailedReprocessingWorker> logger,
			IOptionsMonitor<ReceiptFailedReprocessingOptions> options,
			IServiceScopeFactory serviceScopeFactory,
			IReceiptReprocessingLockCoordinator lockCoordinator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_lockCoordinator = lockCoordinator ?? throw new ArgumentNullException(nameof(lockCoordinator));
		}

		/// <summary>
		/// Интервал задержки до следующего планового запуска по расписанию
		/// </summary>
		protected override TimeSpan Interval => GetDelayToNextScheduledRun();

		/// <inheritdoc/>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{			
			while(!stoppingToken.IsCancellationRequested)
			{
				await DoWork(stoppingToken);

				var delay = Interval;
				var targetTime = DateTime.UtcNow.Add(delay);

				_logger.LogInformation(
					"Ожидание до следующего планового запуска переобработки чеков со статусом Failed: через {Delay:hh\\:mm\\:ss}",
					delay);

				// Отправляем heartbeat Zabbix каждую минуту во время длительного ожидания планового запуска
				while(DateTime.UtcNow < targetTime && !stoppingToken.IsCancellationRequested)
				{
					var remaining = targetTime - DateTime.UtcNow;
					var sleepChunk = remaining > TimeSpan.FromMinutes(1) ? TimeSpan.FromMinutes(1) : remaining;

					if(sleepChunk > TimeSpan.Zero)
					{
						await Task.Delay(sleepChunk, stoppingToken);
					}

					try
					{
						using var scope = _serviceScopeFactory.CreateScope();
						var zabbixSender = scope.ServiceProvider.GetService<IZabbixSender>();
						if(zabbixSender != null)
						{
							await zabbixSender.SendIsHealthyAsync(nameof(ReceiptFailedReprocessingWorker), stoppingToken);
						}
					}
					catch(Exception ex)
					{
						_logger.LogTrace(ex, "Ошибка отправки heartbeat в Zabbix при ожидании");
					}
				}
			}
		}

		/// <summary>
		/// Вычисляет время задержки до ближайшего планового запуска по московскому времени
		/// </summary>
		/// <returns>Временной интервал до следующего запуска</returns>
		private TimeSpan GetDelayToNextScheduledRun()
		{
			var scheduleHours = _options.CurrentValue.ScheduleRunHours;
			if(scheduleHours != null && scheduleHours.Length > 0)
			{
				var validHours = scheduleHours
					.Where(hour => hour >= 0 && hour < 24)
					.Distinct()
					.OrderBy(hour => hour)
					.ToList();

				if(validHours.Count > 0)
				{
					var moscowTimeZone = GetMoscowTimeZone();
					var nowMoscow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);

					var nextRunTimes = validHours
						.Select(hour => nowMoscow.Date.AddHours(hour))
						.Where(runTime => runTime > nowMoscow)
						.OrderBy(runTime => runTime)
						.ToList();

					DateTime nextRun;
					if(nextRunTimes.Count > 0)
					{
						nextRun = nextRunTimes[0];
					}
					else
					{
						var firstHourTomorrow = validHours.Min();
						nextRun = nowMoscow.Date.AddDays(1).AddHours(firstHourTomorrow);
					}

					var delay = nextRun - nowMoscow;
					return delay > TimeSpan.Zero ? delay : TimeSpan.FromMinutes(1);
				}
			}

			// Если часы не заданы или некорректны, используем периодический интервал
			return _options.CurrentValue.WorkerInterval > TimeSpan.Zero
				? _options.CurrentValue.WorkerInterval
				: TimeSpan.FromHours(8);
		}

		/// <summary>
		/// Получает объект часового пояса Москвы с учетом кроссплатформенности (Windows и Linux)
		/// </summary>
		/// <returns>Часовой пояс Москвы</returns>
		private static TimeZoneInfo GetMoscowTimeZone()
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
			}
			catch(TimeZoneNotFoundException)
			{
				return TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
			}
		}

		/// <inheritdoc/>
		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();

			if(!await _lockCoordinator.TryAcquireLockAsync(stoppingToken))
			{
				_logger.LogInformation("Переобработка чеков уже выполняется другим процессом или запросом, плановый запуск пропускается");
				await zabbixSender.SendIsHealthyAsync(nameof(ReceiptFailedReprocessingWorker), stoppingToken);
				return;
			}

			try
			{
				var reprocessingService = scope.ServiceProvider.GetRequiredService<IReceiptFailedReprocessingService>();

				_logger.LogInformation("Запуск плановой переобработки чеков со статусом Failed");

				var result = await reprocessingService.ProcessFailedDocumentsAsync(stoppingToken);

				_logger.LogInformation(
					"Плановая переобработка чеков завершена. Найдено: {Total}, успешно: {Success}, заменено кодов: {Codes}, ошибок: {Errors}",
					result.TotalFound,
					result.SuccessCount,
					result.ReplacedCodesCount,
					result.ErrorCount);

				await zabbixSender.SendIsHealthyAsync(nameof(ReceiptFailedReprocessingWorker), stoppingToken);
			}
			catch(OperationCanceledException)
			{
				_logger.LogInformation("Плановая переобработка чеков была отменена");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выполнении плановой переобработки чеков с ошибками");
			}
			finally
			{
				await _lockCoordinator.ReleaseLockAsync();
			}
		}
	}
}
