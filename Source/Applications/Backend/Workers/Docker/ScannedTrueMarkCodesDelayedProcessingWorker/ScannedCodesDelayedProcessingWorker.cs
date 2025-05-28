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

		public ScannedCodesDelayedProcessingWorker(
			ILogger<ScannedCodesDelayedProcessingWorker> logger,
			IOptions<ScannedCodesDelayedProcessingOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger =
				logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override TimeSpan Interval => _options.Value.ScanInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var zabbixSender = scope.ServiceProvider.GetService<IZabbixSender>();
				var scannedCodesDelayedProcessingService = scope.ServiceProvider.GetService<ScannedCodesDelayedProcessingService>();

				_logger.LogInformation("Обрабатываем сохраненные водителями отсканированные коды ЧЗ");

				try
				{
					await scannedCodesDelayedProcessingService.ProcessScannedCodesAsync(stoppingToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке сохраненных водителями отсканированных кодов ЧЗ");
				}

				await zabbixSender.SendIsHealthyAsync(stoppingToken);

				_logger.LogInformation("Завершение обработки сохраненных водителями кодов ЧЗ");
			}
		}
	}
}
