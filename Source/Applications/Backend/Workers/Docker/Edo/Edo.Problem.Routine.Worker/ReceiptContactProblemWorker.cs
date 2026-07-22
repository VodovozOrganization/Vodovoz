using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace Edo.Problem.Routine.Worker
{
	public class ReceiptContactProblemWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ReceiptContactProblemWorker> _logger;
		private readonly IOptions<ReceiptContactProblemWorkerOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ReceiptContactProblemWorker(
			ILogger<ReceiptContactProblemWorker> logger,
			IOptions<ReceiptContactProblemWorkerOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => _options.Value.WorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();

			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
			var receiptContactProblemService = scope.ServiceProvider.GetRequiredService<ReceiptContactProblemService>();

			_logger.LogInformation("Запуск обработки задач ЭДО с активной проблемой контакта чека");

			try
			{
				await receiptContactProblemService.ProcessContactProblems(stoppingToken);

				_logger.LogInformation("Обработка задач ЭДО с активной проблемой контакта чека успешно завершена");

				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке задач ЭДО с активной проблемой контакта чека");

				await zabbixSender.SendProblemMessageAsync(
					ZabixSenderMessageType.Problem,
					$"Ошибка при обработке задач ЭДО с активной проблемой контакта чека: {ex.Message}",
					stoppingToken);
			}
		}
	}
}
