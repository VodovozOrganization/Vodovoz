using DatabaseServiceWorker.PowerBiWorker.Exporters;
using DatabaseServiceWorker.PowerBiWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker.PowerBiWorker
{
	internal class PowerBiExportWorker : TimerBackgroundServiceBase
	{
		private bool _workInProgress;
		private readonly ILogger<PowerBiExportWorker> _logger;
		private readonly IPowerBiExporter _powerBiExporter;
		private readonly IZabbixSender _zabbixSender;

		public PowerBiExportWorker(
			ILogger<PowerBiExportWorker> logger,
			IOptions<PowerBiExportOptions> options,
			IPowerBiExporter powerBiExporter,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_powerBiExporter = powerBiExporter ?? throw new ArgumentNullException(nameof(powerBiExporter));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			Interval = options.Value.Interval;
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {TransferStartTime}",
				nameof(PowerBiExportWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(PowerBiExportWorker),
				DateTimeOffset.Now);

			base.OnStopService();
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
				_logger.LogInformation("Начало экспорта в бд PowerBi {PowerBiExportDate}", DateTime.Now);

				await _powerBiExporter.Export(stoppingToken);

				await _zabbixSender.SendIsHealthyAsync(stoppingToken);

				_logger.LogInformation("Экспорт в бд PowerBi завершён.");
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при при эскпорте из БД");

				await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, "Ошибка экспорта в PowerBI.", stoppingToken);
			}
			finally
			{
				_workInProgress = false;
			}

			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayInMinutes}' перед следующим запуском", nameof(PowerBiExportWorker), Interval);

			await Task.CompletedTask;
		}
	}
}
