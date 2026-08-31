using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services.TaxcomSendProblem;
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
	public class TaxcomSendProblemWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<TaxcomSendProblemWorker> _logger;
		private readonly IOptions<TaxcomSendProblemWorkerOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public TaxcomSendProblemWorker(
			ILogger<TaxcomSendProblemWorker> logger,
			IOptions<TaxcomSendProblemWorkerOptions> options,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override TimeSpan Interval => _options.Value.WorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var taxcomSendProblemService = scope.ServiceProvider.GetRequiredService<ITaxcomSendProblemService>();

			_logger.LogInformation("Запуск обработки задач ЭДО с ошибкой отправки в Такском");

			try
			{
				await taxcomSendProblemService.ProcessProblemTasks(stoppingToken);

				_logger.LogInformation("Обработка задач ЭДО с ошибкой отправки в Такском успешно завершена");

				await _zabbixSender.SendIsHealthyAsync(nameof(TaxcomSendProblemWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке задач ЭДО с ошибкой отправки в Такском");
			}
		}
	}
}
