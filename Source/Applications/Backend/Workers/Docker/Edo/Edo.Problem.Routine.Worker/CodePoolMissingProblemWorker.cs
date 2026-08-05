using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services.CodePoolMissingProblem;
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
		private readonly IZabbixSender _zabbixSender;

		public CodePoolMissingProblemWorker(
			ILogger<CodePoolMissingProblemWorker> logger,
			IOptions<CodePoolMissingProblemWorkerOptions> options,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender
		)
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
			var orderEdoCodePoolMissingProblemService = scope.ServiceProvider.GetRequiredService<ICodePoolMissingProblemService>();

			_logger.LogInformation("Запуск обработки задач ЭДО с ошибкой нехватки кодов в пуле");

			try
			{
				await orderEdoCodePoolMissingProblemService.ProcessProblemTasks(stoppingToken);

				_logger.LogInformation("Обработка задач ЭДО с ошибкой нехватки кодов в пуле успешно завершена");

				await _zabbixSender.SendIsHealthyAsync(nameof(CodePoolMissingProblemWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке задач ЭДО с ошибкой нехватки кодов в пуле");

				await _zabbixSender.SendProblemMessageAsync(
					nameof(CodePoolMissingProblemWorker),
					ZabixSenderMessageType.Problem,
					$"Ошибка при обработке задач ЭДО с ошибкой нехватки кодов в пуле: {ex.Message}",
					stoppingToken);
			}
		}
	}
}
