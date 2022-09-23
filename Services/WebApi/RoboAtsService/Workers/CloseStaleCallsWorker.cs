using Microsoft.Extensions.Logging;
using RoboAtsService.Monitoring;
using System;
using Vodovoz.Infrastructure;
using Vodovoz.Parameters;

namespace RoboAtsService.Workers
{
	public class CloseStaleCallsWorker : TimerServiceBase
	{
		private readonly ILogger<CloseStaleCallsWorker> _logger;
		private readonly RoboatsSettings _roboatsSettings;
		private readonly RoboatsCallRegistrator _roboatsCallRegistrator;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public CloseStaleCallsWorker(ILogger<CloseStaleCallsWorker> logger, RoboatsSettings roboatsSettings, RoboatsCallRegistrator roboatsCallRegistrator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_roboatsCallRegistrator = roboatsCallRegistrator ?? throw new ArgumentNullException(nameof(roboatsCallRegistrator));
			_interval = TimeSpan.FromMinutes(_roboatsSettings.StaleCallCheckInterval);
		}
		protected override TimeSpan Interval => _interval;

		protected override void DoWork()
		{
			if(_isRunning)
			{
				return;
			}

			_isRunning = true;

			try
			{
				_logger.LogInformation("Вызов закрытия устаревших звонков");
				_roboatsCallRegistrator.CloseStaleCalls();
			}
			finally
			{
				_isRunning = false;
			}
		}

		protected override void OnStartService()
		{
			_logger.LogInformation("Запущен сервис закрытия устаревших звонков");
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлен сервис закрытия устаревших звонков");
		}
	}
}
