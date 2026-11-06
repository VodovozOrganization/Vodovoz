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
		private static int _isRunning;
		private readonly ILogger<ReceiptFailedReprocessingWorker> _logger;
		private readonly IOptionsMonitor<ReceiptFailedReprocessingOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ReceiptFailedReprocessingWorker(
			ILogger<ReceiptFailedReprocessingWorker> logger,
			IOptionsMonitor<ReceiptFailedReprocessingOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		/// <summary>
		/// Признак того, что процесс переобработки чеков уже выполняется в данный момент
		/// </summary>
		public static bool IsRunning => _isRunning == 1;

		/// <summary>
		/// Попытка захватить атомарную блокировку выполнения
		/// </summary>
		/// <returns>True, если блокировка успешно захвачена</returns>
		public static bool TryAcquireRunningLock() => Interlocked.CompareExchange(ref _isRunning, 1, 0) == 0;

		/// <summary>
		/// Освобождение атомарной блокировки выполнения
		/// </summary>
		public static void ReleaseRunningLock() => Interlocked.Exchange(ref _isRunning, 0);

		/// <summary>
		/// Интервал задержки до следующего планового запуска по расписанию
		/// </summary>
		protected override TimeSpan Interval => GetDelayToNextScheduledRun();

		/// <summary>
		/// Вычисляет время задержки до ближайшего планового запуска (в 08:00 или 17:00 по московскому времени)
		/// </summary>
		/// <returns>Временной интервал до следующего запуска</returns>
		private TimeSpan GetDelayToNextScheduledRun()
		{
			var scheduleHours = _options.CurrentValue.ScheduleRunHours;
			if(scheduleHours != null && scheduleHours.Length > 0)
			{
				// Часовой пояс МСК (UTC+3)
				var moscowOffset = TimeSpan.FromHours(3);
				var nowMoscow = DateTimeOffset.UtcNow.ToOffset(moscowOffset);

				var nextRunTimes = scheduleHours
					.Select(hour => nowMoscow.Date.AddHours(hour))
					.Where(runTime => runTime > nowMoscow.DateTime)
					.OrderBy(runTime => runTime)
					.ToList();

				DateTime nextRun;
				if(nextRunTimes.Count > 0)
				{
					nextRun = nextRunTimes[0];
				}
				else
				{
					var firstHourTomorrow = System.Linq.Enumerable.Min(scheduleHours);
					nextRun = nowMoscow.Date.AddDays(1).AddHours(firstHourTomorrow);
				}

				var delay = nextRun - nowMoscow.DateTime;
				_logger.LogInformation(
					"Следующий плановый запуск переобработки чеков со статусом Failed назначен на {NextRun:yyyy-MM-dd HH:mm:ss} MSK (через {Delay})",
					nextRun,
					delay);

				return delay > TimeSpan.Zero ? delay : TimeSpan.FromMinutes(1);
			}

			// Если часы не заданы, используем интервал
			return _options.CurrentValue.WorkerInterval > TimeSpan.Zero
				? _options.CurrentValue.WorkerInterval
				: TimeSpan.FromMinutes(30);
		}

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(!TryAcquireRunningLock())
			{
				_logger.LogInformation("Плановая переобработка чеков уже выполняется другим потоком, пропускаем");
				return;
			}

			try
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
				var reprocessingService = scope.ServiceProvider.GetRequiredService<IReceiptFailedReprocessingService>();

				_logger.LogInformation("Запуск плановой переобработки чеков со статусом Failed");

				var result = await reprocessingService.ProcessFailedDocumentsAsync(stoppingToken);

				_logger.LogInformation(
					"Плановая переобработка чеков завершена. Найдено: {Total}, успешно: {Success}, заменено кодов: {Codes}, закрытый период: {Closed}, ошибок: {Errors}",
					result.TotalFound,
					result.SuccessCount,
					result.ReplacedCodesCount,
					result.SkippedClosedPeriodCount,
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
				ReleaseRunningLock();
			}
		}
	}
}
