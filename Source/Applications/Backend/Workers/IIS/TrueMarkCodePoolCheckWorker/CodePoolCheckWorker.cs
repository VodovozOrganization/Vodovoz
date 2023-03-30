using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Edo;

namespace TrueMarkCodesWorker
{
	public class CodePoolCheckWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<CodePoolCheckWorker> _logger;
		private readonly TrueMarkCodePoolChecker _trueMarkCodePoolChecker;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public CodePoolCheckWorker(
			ILogger<CodePoolCheckWorker> logger, 
			IEdoSettings edoSettings,
			TrueMarkCodePoolChecker trueMarkCodePoolChecker
		)
		{
			if(edoSettings is null)
			{
				throw new ArgumentNullException(nameof(edoSettings));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkCodePoolChecker = trueMarkCodePoolChecker ?? throw new ArgumentNullException(nameof(trueMarkCodePoolChecker));
			_interval = TimeSpan.FromMinutes(edoSettings.CodePoolCheckIntervalMinutes);
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
				_logger.LogInformation("Вызов проверки кодов из пула в системе честный знак");
				await _trueMarkCodePoolChecker.StartCheck(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogCritical(ex, "Поймано необработанное исключение");
			}
			finally
			{
				_isRunning = false;
			}
		}

		protected override void OnStartService()
		{
			_logger.LogInformation("Запущен сервис проверки кодов из пула в системе честный знак");
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлен сервис проверки кодов из пула в системе честный знак");
		}
	}
}
