using Edo.Common;
using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Edo;
using Vodovoz.Zabbix.Sender;

namespace Edo.Receipt.NightSend
{
	public class ReceiptNightSendProblemWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ReceiptNightSendProblemWorker> _logger;
		private readonly IOptionsMonitor<ReceiptNightSendProblemWorkerOptions> _options;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ReceiptNightSendProblemWorker(
			ILogger<ReceiptNightSendProblemWorker> logger,
			IOptionsMonitor<ReceiptNightSendProblemWorkerOptions> options,
			IEdoReceiptSettings edoReceiptSettings,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => _options.CurrentValue.WorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(ReceiptSendPauseTimeHelper.IsNightPauseTime(
				DateTime.Now.TimeOfDay,
				_edoReceiptSettings.ReceiptSendPauseStartTime,
				_edoReceiptSettings.ReceiptSendPauseEndTime))
			{
				return;
			}

			using var scope = _serviceScopeFactory.CreateScope();

			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
			var receiptNightSendProblemService = scope.ServiceProvider.GetRequiredService<ReceiptNightSendProblemService>();

			_logger.LogInformation("Запуск обработки чеков, отложенных из-за ночного времени");

			try
			{
				await receiptNightSendProblemService.ProcessNightSendProblems(stoppingToken);

				_logger.LogInformation("Обработка чеков, отложенных из-за ночного времени, успешно завершена");

				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке чеков, отложенных из-за ночного времени");

				await zabbixSender.SendProblemMessageAsync(
					ZabixSenderMessageType.Problem,
					$"Ошибка при обработке чеков, отложенных из-за ночного времени: {ex.Message}",
					stoppingToken);
			}
		}

	}
}
