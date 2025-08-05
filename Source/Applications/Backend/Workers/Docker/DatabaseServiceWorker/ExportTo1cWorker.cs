using DatabaseServiceWorker.PowerBiWorker.Exporters;
using DatabaseServiceWorker.PowerBiWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker
{
	internal class ExportTo1cWorker : TimerBackgroundServiceBase
	{
		private bool _workInProgress;
		private readonly ILogger<ExportTo1cWorker> _logger;		
		private readonly IZabbixSender _zabbixSender;

		public ExportTo1cWorker(
			ILogger<ExportTo1cWorker> logger,
			IOptions<PowerBiExportOptions> options,			
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));			
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			Interval = options.Value.Interval;
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {TransferStartTime}",
				nameof(ExportTo1cWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(ExportTo1cWorker),
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
				_logger.LogInformation("Начало экспорта данных 1С из бд в файл {Export1cExportDate}", DateTime.Now);

				await Export(stoppingToken);

				await _zabbixSender.SendIsHealthyAsync(stoppingToken);

				_logger.LogInformation("Экспорт данных 1С из бд в файл завершён.");
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при при эскпорте данных 1С из бд в файл");

				await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, "Ошибка экспорта данных для 1c.", stoppingToken);
			}
			finally
			{
				_workInProgress = false;
			}

			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayInMinutes}' перед следующим запуском", nameof(ExportTo1cWorker), Interval);

			await Task.CompletedTask;
		}

		private async Task Export(CancellationToken stoppingToken)
		{
			
		}
	}
}
