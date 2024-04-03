using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Models;
using Vodovoz.Settings.Delivery;

namespace DatabaseServiceWorker
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
				_logger.LogInformation(
					"Воркер ClearFastDeliveryAvailabilityHistoryWorker запущен в: {time}",
					DateTimeOffset.Now);

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
					_logger.LogError(
						e,
						"Ошибка при выполнении очистки истории проверок доставки за час {TodayDate}",
						DateTime.Today.ToString("dd-MM-yyyy"));
				}
				finally
				{
					_workInProgress = false;
				}
				
				_logger.LogInformation(
					"Воркер ClearFastDeliveryAvailabilityHistoryWorker ожидает {DelayInMinutes}мин перед следующим запуском",
					_delayInMinutes);
				
				await Task.Delay(TimeSpan.FromMinutes(_delayInMinutes), stoppingToken);
			}
		}

		private void ClearFastDeliveryAvailabilityHistory()
		{
			_fastDeliveryAvailabilityHistoryModel.ClearFastDeliveryAvailabilityHistory(_fastDeliveryAvailabilityHistorySettings);
		}
	}
}
