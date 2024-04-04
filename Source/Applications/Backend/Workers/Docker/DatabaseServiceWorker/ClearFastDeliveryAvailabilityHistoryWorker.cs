using DatabaseServiceWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Models;
using Vodovoz.Settings.Delivery;

namespace DatabaseServiceWorker
{
	public class ClearFastDeliveryAvailabilityHistoryWorker : TimerBackgroundServiceBase
	{
		private readonly IOptions<ClearFastDeliveryAvailabilityHistoryOptions> _options;
		private readonly ILogger<ClearFastDeliveryAvailabilityHistoryWorker> _logger;
		private readonly IFastDeliveryAvailabilityHistoryModel _fastDeliveryAvailabilityHistoryModel;
		private readonly IFastDeliveryAvailabilityHistorySettings _fastDeliveryAvailabilityHistorySettings;

		private bool _workInProgress;

		public ClearFastDeliveryAvailabilityHistoryWorker(
			IOptions<ClearFastDeliveryAvailabilityHistoryOptions> options,
			ILogger<ClearFastDeliveryAvailabilityHistoryWorker> logger,
			IFastDeliveryAvailabilityHistoryModel fastDeliveryAvailabilityHistoryModel,
			IFastDeliveryAvailabilityHistorySettings fastDeliveryAvailabilityHistorySettings)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastDeliveryAvailabilityHistoryModel = fastDeliveryAvailabilityHistoryModel ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistoryModel));
			_fastDeliveryAvailabilityHistorySettings = fastDeliveryAvailabilityHistorySettings ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistorySettings));

			Interval = _options.Value.ScanInterval;
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
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
				"Воркер ClearFastDeliveryAvailabilityHistoryWorker ожидает '{DelayInMinutes}' перед следующим запуском",
				Interval);

			await Task.CompletedTask;
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер ClearFastDeliveryAvailabilityHistoryWorker запущен в: {time}",
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер ClearFastDeliveryAvailabilityHistoryWorker завершил работу в: {time}",
				DateTimeOffset.Now);

			base.OnStopService();
		}

		private void ClearFastDeliveryAvailabilityHistory()
		{
			_fastDeliveryAvailabilityHistoryModel.ClearFastDeliveryAvailabilityHistory(
				_fastDeliveryAvailabilityHistorySettings,
				_options.Value.DeleteQueryTimeout);
		}
	}
}
