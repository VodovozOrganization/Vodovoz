using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Edo;

namespace TrueMarkCodePoolCheckWorker
{
	public class ExpiredCodesCleanerWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ExpiredCodesCleanerWorker> _logger;
		private readonly ITrueMarkCodesPoolManager _trueMarkCodePoolManager;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public ExpiredCodesCleanerWorker(
			ILogger<ExpiredCodesCleanerWorker> logger,
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
			_interval = TimeSpan.FromMinutes(edoSettings.ExpiredCodesCleanerIntervalMinutes);
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
				_logger.LogInformation("Начало очистки пула от просроченных кодов");
				var deletedCount = await _trueMarkCodePoolManager.DeleteExpiredCodesAsync(stoppingToken);

				if(deletedCount > 0)
				{
					_logger.LogInformation("Удалено {Count} просроченных кодов из пула", deletedCount);
				}
				else
				{
					_logger.LogInformation("Просроченных кодов для удаления не найдено");
				}
			}
			catch(Exception ex)
			{
				_logger.LogCritical(ex, "Поймано необработанное исключение при очистке просроченных кодов");
			}
			finally
			{
				_isRunning = false;
			}
		}

		protected override void OnStartService()
		{
			_logger.LogInformation("Запущен сервис очистки просроченных кодов из пула");
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлен сервис очистки просроченных кодов из пула");
		}
	}
}
