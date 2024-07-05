using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Tools;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker
{
	public class MonitoringArchivingWorker : BackgroundService
	{
		private const int _delayInMinutes = 20;
		private readonly ILogger<MonitoringArchivingWorker> _logger;
		private readonly IDataArchiver _archiver;
		private readonly IZabbixSender _zabbixSender;
		private bool _workInProgress;

		public MonitoringArchivingWorker(ILogger<MonitoringArchivingWorker> logger, IDataArchiver archiver, IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_archiver = archiver ?? throw new ArgumentNullException(nameof(archiver));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

				if(DateTime.Now.Hour >= 4 && DateTime.Now.Hour < 8)
				{
					if(_workInProgress)
					{
						return;
					}

					_workInProgress = true;

					try
					{
						ArchiveMonitoring();
						ArchiveTrackPoints();
						DeleteDistanceCache();
					}
					catch(Exception e)
					{
						_logger.LogError(e, $"Ошибка при выполнении процесса архивации всех сущностей {DateTime.Today:dd-MM-yyyy}");
					}
					finally
					{
						_workInProgress = false;
					}
				}

				await _zabbixSender.SendIsHealthyAsync();

				_logger.LogInformation($"Ожидаем {_delayInMinutes}мин перед следующим запуском");
				await Task.Delay(1000 * 60 * _delayInMinutes, stoppingToken);
			}
		}

		private void ArchiveMonitoring()
		{
			_archiver.ArchiveMonitoring();
		}

		private void ArchiveTrackPoints()
		{
			_archiver.ArchiveTrackPoints();
		}

		private void DeleteDistanceCache()
		{
			_archiver.DeleteDistanceCache();
		}
	}
}
