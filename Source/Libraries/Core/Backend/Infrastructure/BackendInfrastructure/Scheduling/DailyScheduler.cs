using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Infrastructure.Scheduling
{
	public class DailyScheduler : IDailyScheduler
	{
		private readonly ILogger<DailyScheduler> _logger;

		public DailyScheduler(ILogger<DailyScheduler> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task DelayUntilNextOccurrenceAsync(
			TimeSpan timeOfDay,
			string workerName,
			CancellationToken cancellationToken = default)
		{
			if(string.IsNullOrWhiteSpace(workerName))
			{
				workerName = "UnknownWorker";
			}

			if(timeOfDay < TimeSpan.Zero || timeOfDay >= TimeSpan.FromDays(1))
			{
				throw new ArgumentException(
					"Время суток должно быть в диапазоне от 00:00:00 до 23:59:59",
					nameof(timeOfDay));
			}

			try
			{
				var now = DateTime.Now;
				var todayAtSpecifiedTime = now.Date + timeOfDay;
				var nextRun = now >= todayAtSpecifiedTime
					? todayAtSpecifiedTime.AddDays(1)
					: todayAtSpecifiedTime;

				var delay = nextRun - now;

				_logger.LogInformation(
					"[{WorkerName}] Сейчас {CurrentTime}, запуск в {TimeOfDay}, " +
					"следующий запуск {NextRunTime} (через {Delay})",
					workerName,
					now.ToString("yyyy-MM-dd HH:mm:ss"),
					timeOfDay,
					nextRun.ToString("yyyy-MM-dd HH:mm:ss"),
					delay);

				await Task.Delay(delay, cancellationToken);

				_logger.LogDebug("[{WorkerName}] Задержка завершена, начинаем следующий цикл", workerName);
			}
			catch(OperationCanceledException)
			{
				_logger.LogInformation("[{WorkerName}] Ожидание отменено (воркер останавливается)", workerName);
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "[{WorkerName}] Ошибка в DailyScheduler", workerName);
				throw;
			}
		}
	}
}
