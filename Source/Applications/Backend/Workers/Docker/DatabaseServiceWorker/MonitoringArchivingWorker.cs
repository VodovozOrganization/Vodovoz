using Microsoft.Extensions.DependencyInjection;
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
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private bool _workInProgress;

		public MonitoringArchivingWorker(ILogger<MonitoringArchivingWorker> logger, IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

				using var scope = _serviceScopeFactory.CreateScope();
				var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();

				if(DateTime.Now.Hour >= 4 && DateTime.Now.Hour < 8)
				{
					if(_workInProgress)
					{
						return;
					}

					_workInProgress = true;

					try
					{
						var dataArchiver = scope.ServiceProvider.GetRequiredService<IDataArchiver>();
						ArchiveMonitoring(dataArchiver);
						ArchiveTrackPoints(dataArchiver);
						DeleteDistanceCache(dataArchiver);
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

				await zabbixSender.SendIsHealthyAsync(stoppingToken);

				_logger.LogInformation($"Ожидаем {_delayInMinutes}мин перед следующим запуском");
				await Task.Delay(1000 * 60 * _delayInMinutes, stoppingToken);
			}
		}

		private void ArchiveMonitoring(IDataArchiver dataArchiver)
		{
			dataArchiver.ArchiveMonitoring();
		}

		private void ArchiveTrackPoints(IDataArchiver dataArchiver)
		{
			dataArchiver.ArchiveTrackPoints();
		}

		private void DeleteDistanceCache(IDataArchiver dataArchiver)
		{
			dataArchiver.DeleteDistanceCache();
		}
	}
}
