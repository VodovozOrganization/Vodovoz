using Microsoft.Extensions.Logging;
using RoboatsCallsWorker;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Roboats;
using Vodovoz.Zabbix.Sender;

namespace RoboatsService.Workers
{
	public class CloseStaleCallsWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<CloseStaleCallsWorker> _logger;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly StaleCallsController _staleCallsController;
		private readonly IZabbixSender _zabbixSender;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public CloseStaleCallsWorker(
			ILogger<CloseStaleCallsWorker> logger,
			IRoboatsSettings roboatsSettings,
			StaleCallsController staleCallsController,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_staleCallsController = staleCallsController ?? throw new ArgumentNullException(nameof(staleCallsController));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_interval = TimeSpan.FromMinutes(_roboatsSettings.StaleCallCheckInterval);
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
				_logger.LogInformation("Вызов закрытия устаревших звонков");
				_staleCallsController.CloseStaleCalls();
				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
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
