using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Models;
using Vodovoz.Settings.Delivery;

namespace MonitoringArchivingWorker
{
	public class ClearFastDeliveryAvailabilityHistoryWorker : BackgroundService
	{
		private const int _delayInMinutes = 120;

		private readonly ILogger<ClearFastDeliveryAvailabilityHistoryWorker> _logger;
		private readonly IFastDeliveryAvailabilityHistoryModel _fastDeliveryAvailabilityHistoryModel;
		private readonly IFastDeliveryAvailabilityHistorySettings _fastDeliveryAvailabilityHistorySettings;

		private bool _workInProgress;

		public ClearFastDeliveryAvailabilityHistoryWorker(
			ILogger<ClearFastDeliveryAvailabilityHistoryWorker> logger,
			IFastDeliveryAvailabilityHistoryModel fastDeliveryAvailabilityHistoryModel,
			IFastDeliveryAvailabilityHistorySettings fastDeliveryAvailabilityHistorySettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastDeliveryAvailabilityHistoryModel = fastDeliveryAvailabilityHistoryModel ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistoryModel));
			_fastDeliveryAvailabilityHistorySettings = fastDeliveryAvailabilityHistorySettings ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistorySettings));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Воркер очистки истории проверок доставки за час запущен в: {time}", DateTimeOffset.Now);

				if(_workInProgress)
				{
					return;
				}

				_workInProgress = true;

				try
				{
					ClearFastDeliveryAvailabilityHistory();
				}
				catch(Exception e)
				{
					_logger.LogError(e, $"Ошибка при выполнении очистки истории проверок доставки за час {DateTime.Today:dd-MM-yyyy}");
				}
				finally
				{
					_workInProgress = false;
				}
				
				_logger.LogInformation($"Воркер очистки истории проверок доставки за час ожидает {_delayInMinutes}мин перед следующим запуском");
				
				await Task.Delay(1000 * 60 * _delayInMinutes, stoppingToken);
			}
		}

		private void ClearFastDeliveryAvailabilityHistory()
		{
			_fastDeliveryAvailabilityHistoryModel.ClearFastDeliveryAvailabilityHistory(_fastDeliveryAvailabilityHistorySettings);
		}
	}
}
