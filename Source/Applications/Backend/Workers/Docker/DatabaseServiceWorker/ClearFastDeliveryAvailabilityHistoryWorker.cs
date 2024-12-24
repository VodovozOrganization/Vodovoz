using DatabaseServiceWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Models;
using Vodovoz.Settings.Delivery;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker
{
	public class ClearFastDeliveryAvailabilityHistoryWorker : TimerBackgroundServiceBase
	{
		private readonly IOptions<ClearFastDeliveryAvailabilityHistoryOptions> _options;
		private readonly ILogger<ClearFastDeliveryAvailabilityHistoryWorker> _logger;
		private readonly IFastDeliveryAvailabilityHistoryModel _fastDeliveryAvailabilityHistoryModel;
		private readonly IFastDeliveryAvailabilityHistorySettings _fastDeliveryAvailabilityHistorySettings;
		private readonly IZabbixSender _zabbixSender;
		private bool _workInProgress;
		private DateTime? _lastClearDate;

		public ClearFastDeliveryAvailabilityHistoryWorker(
			IOptions<ClearFastDeliveryAvailabilityHistoryOptions> options,
			ILogger<ClearFastDeliveryAvailabilityHistoryWorker> logger,
			IFastDeliveryAvailabilityHistoryModel fastDeliveryAvailabilityHistoryModel,
			IFastDeliveryAvailabilityHistorySettings fastDeliveryAvailabilityHistorySettings,
			IZabbixSender zabbixSender)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastDeliveryAvailabilityHistoryModel = fastDeliveryAvailabilityHistoryModel ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistoryModel));
			_fastDeliveryAvailabilityHistorySettings = fastDeliveryAvailabilityHistorySettings ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistorySettings));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
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
				if(_lastClearDate.HasValue && _lastClearDate >= DateTime.Today)
				{
					_logger.LogInformation("Удаление записей истории проверки доступности экспресс-доставки не требуется. Дата последней очистки: '{LastClearDate}'. Дата сейчас: '{NowDate}'",
					_lastClearDate.HasValue ? _lastClearDate.Value.ToString("yyyy-MM-dd") : "Не выполнялось",
					DateTime.Now.ToString("yyyy-MM-dd"));

					return;
				}

				ClearFastDeliveryAvailabilityHistory();
				_lastClearDate = DateTime.Today;

				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception e)
			{
				_lastClearDate = null;

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
				"Воркер ClearFastDeliveryAvailabilityHistoryWorker запущен в: {TransferStartTime}",
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер ClearFastDeliveryAvailabilityHistoryWorker завершил работу в: {StopTime}",
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
