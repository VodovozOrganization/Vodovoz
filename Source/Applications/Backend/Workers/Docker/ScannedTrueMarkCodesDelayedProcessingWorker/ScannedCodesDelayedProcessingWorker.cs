using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScannedTrueMarkCodesDelayedProcessing.Library.Option;
using ScannedTrueMarkCodesDelayedProcessing.Library.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace ScannedTrueMarkCodesDelayedProcessingWorker
{
	public class ScannedCodesDelayedProcessingWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ScannedCodesDelayedProcessingWorker> _logger;
		private readonly IOptions<ScannedCodesDelayedProcessingOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public ScannedCodesDelayedProcessingWorker(
			ILogger<ScannedCodesDelayedProcessingWorker> logger,
			IOptions<ScannedCodesDelayedProcessingOptions> options,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override TimeSpan Interval => _options.Value.ScanInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				await ProcessScannedCodesInScopeAsync(stoppingToken);
			}
			catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Обработка сохраненных водителями кодов ЧЗ прервана остановкой воркера");
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка итерации обработки сохраненных водителями кодов ЧЗ, повтор через {Interval}",
					Interval);

				await SendProblemAsync($"Ошибка итерации обработки сохраненных водителями кодов ЧЗ: {ex.Message}", stoppingToken);
			}
		}

		private async Task ProcessScannedCodesInScopeAsync(CancellationToken stoppingToken)
		{
			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var scannedCodesDelayedProcessingService = scope.ServiceProvider.GetService<ScannedCodesDelayedProcessingService>();

				_logger.LogInformation("Обрабатываем сохраненные водителями отсканированные коды ЧЗ");

				try
				{
					await scannedCodesDelayedProcessingService.ProcessScannedCodesAsync(stoppingToken);

					await _zabbixSender.SendIsHealthyAsync(nameof(ScannedCodesDelayedProcessingWorker), stoppingToken);
				}
				catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
				{
					_logger.LogInformation("Обработка сохраненных водителями кодов ЧЗ прервана остановкой воркера");
					return;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке сохраненных водителями отсканированных кодов ЧЗ");

					await SendProblemAsync($"Ошибка при обработке сохраненных водителями кодов ЧЗ: {ex.Message}", stoppingToken);
				}

				_logger.LogInformation("Завершение обработки сохраненных водителями кодов ЧЗ");
			}
		}

		private async Task SendProblemAsync(string message, CancellationToken stoppingToken)
		{
			try
			{
				await _zabbixSender.SendProblemMessageAsync(
					nameof(ScannedCodesDelayedProcessingWorker),
					ZabixSenderMessageType.Problem,
					message,
					stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки сообщения о проблеме в zabbix");
			}
		}
	}
}
