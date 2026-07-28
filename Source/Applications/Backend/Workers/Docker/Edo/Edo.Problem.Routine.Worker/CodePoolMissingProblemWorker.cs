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
	public class CodePoolMissingProblemWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<CodePoolMissingProblemWorker> _logger;
		private readonly IOptions<CodePoolMissingProblemWorkerOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public CodePoolMissingProblemWorker(
			ILogger<CodePoolMissingProblemWorker> logger,
			IOptions<CodePoolMissingProblemWorkerOptions> options,
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
			var orderEdoCodePoolMissingProblemService = scope.ServiceProvider.GetRequiredService<OrderEdoCodePoolMissingProblemService>();

			_logger.LogInformation("Запуск обработки задач ЭДО с ошибкой нехватки кодов в пуле");

			try
			{
				await orderEdoCodePoolMissingProblemService.ProcessProblemTasks(stoppingToken);

				_logger.LogInformation("Обработка задач ЭДО с ошибкой нехватки кодов в пуле успешно завершена");

				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке задач ЭДО с ошибкой нехватки кодов в пуле");

				await zabbixSender.SendProblemMessageAsync(
					ZabixSenderMessageType.Problem,
					$"Ошибка при обработке задач ЭДО с ошибкой нехватки кодов в пуле: {ex.Message}",
					stoppingToken);
			}
		}
	}
}
