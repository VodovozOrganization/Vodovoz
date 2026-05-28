using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace Edo.Problem.Routine.Worker
{
	public class FiscalDocumentSendErrorProblemWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<FiscalDocumentSendErrorProblemWorker> _logger;
		private readonly IOptionsMonitor<FiscalDocumentSendErrorProblemWorkerOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public FiscalDocumentSendErrorProblemWorker(
			ILogger<FiscalDocumentSendErrorProblemWorker> logger,
			IOptionsMonitor<FiscalDocumentSendErrorProblemWorkerOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => _options.CurrentValue.WorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
			var fiscalDocumentSendErrorProblemService = scope.ServiceProvider.GetRequiredService<FiscalDocumentSendErrorProblemService>();

			_logger.LogInformation("Запуск обработки фискальных документов с проблемой отправки");

			try
			{
				await fiscalDocumentSendErrorProblemService.ProcessProblemFiscalDocuments(stoppingToken);

				_logger.LogInformation("Обработка фискальных документов с проблемой отправки успешно завершена");

				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке фискальных документов с проблемой отправки");
			}
		}
	}
}
