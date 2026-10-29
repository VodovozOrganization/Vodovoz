using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Edo;

namespace TrueMarkCodePoolCheckWorker
{
	public class UsedCodesCleanerWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<UsedCodesCleanerWorker> _logger;
		private readonly ITrueMarkCodesPoolManager _trueMarkCodePoolManager;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public UsedCodesCleanerWorker(
			ILogger<UsedCodesCleanerWorker> logger,
			IEdoSettings edoSettings,
			ITrueMarkCodesPoolManager trueMarkCodesPoolManager
		)
		{
			if(edoSettings is null)
			{
				throw new ArgumentNullException(nameof(edoSettings));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkCodePoolManager = trueMarkCodesPoolManager ?? throw new ArgumentNullException(nameof(trueMarkCodesPoolManager));
			_interval = TimeSpan.FromHours(edoSettings.UsedCodesCleanerIntervalHours);
		}

		protected override TimeSpan Interval => _interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(_isRunning)
			{
				return;
			}

			_isRunning = true;

			try
			{
				_logger.LogInformation("Начало очистки пула от использованных кодов");
				var deletedCount = await _trueMarkCodePoolManager.DeleteUsedCodesAsync(stoppingToken);

				if(deletedCount > 0)
				{
					_logger.LogInformation("Удалено {Count} использованных кодов из пула", deletedCount);
				}
				else
				{
					_logger.LogInformation("Использованных кодов для удаления не найдено");
				}
			}
			catch(Exception ex)
			{
				_logger.LogCritical(ex, "Поймано необработанное исключение при очистке использованных кодов");
			}
			finally
			{
				_isRunning = false;
			}
		}

		protected override void OnStartService()
		{
			_logger.LogInformation("Запущен сервис очистки использованных кодов из пула");
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлен сервис очистки использованных кодов из пула");
		}
	}
}
