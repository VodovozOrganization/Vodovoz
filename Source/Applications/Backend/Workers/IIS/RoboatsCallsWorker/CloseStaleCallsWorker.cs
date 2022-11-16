using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RoboatsCallsWorker;
using System;
using System.Threading;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Infrastructure;
using Vodovoz.Parameters;

namespace RoboAtsService.Workers
{
	public class CloseStaleCallsWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<CloseStaleCallsWorker> _logger;
		private readonly RoboatsSettings _roboatsSettings;
		private readonly StaleCallsController _staleCallsController;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public CloseStaleCallsWorker(ILogger<CloseStaleCallsWorker> logger, RoboatsSettings roboatsSettings, StaleCallsController staleCallsController)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_staleCallsController = staleCallsController ?? throw new ArgumentNullException(nameof(staleCallsController));
			_interval = TimeSpan.FromMinutes(_roboatsSettings.StaleCallCheckInterval);
		}
		protected override TimeSpan Interval => _interval;

		protected override void DoWork(CancellationToken stoppingToken)
		{
			if(_isRunning)
			{
				return;
			}

			_isRunning = true;

			try
			{
				_logger.LogInformation("Вызов закрытия устаревших звонков");
				_staleCallsController.CloseStaleCalls();
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
