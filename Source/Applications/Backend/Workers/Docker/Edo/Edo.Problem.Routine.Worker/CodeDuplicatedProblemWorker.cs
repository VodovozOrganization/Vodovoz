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
	public class CodeDuplicatedProblemWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<CodeDuplicatedProblemWorker> _logger;
		private readonly IOptions<CodeDuplicatedProblemWorkerOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private const string _workerName = "Дубликат кода";

		public CodeDuplicatedProblemWorker(
			ILogger<CodeDuplicatedProblemWorker> logger,
			IOptions<CodeDuplicatedProblemWorkerOptions> options,
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
			var codeDuplicatedProblemService = scope.ServiceProvider.GetRequiredService<CodeDuplicatedProblemService>();

			_logger.LogInformation($"Запуск обработки задач ЭДО с активной проблемой {_workerName}");

			try
			{
				await codeDuplicatedProblemService.ProcessProblemTasks(stoppingToken);

				_logger.LogInformation($"Обработка задач ЭДО с активной проблемой {_workerName} успешно завершена");

				await zabbixSender.SendIsHealthyAsync(nameof(CodeDuplicatedProblemWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при обработке задач ЭДО с активной проблемой {_workerName}");

				await zabbixSender.SendProblemMessageAsync(nameof(CodeDuplicatedProblemWorker), ZabixSenderMessageType.Problem,
					$"Ошибка при обработке задач ЭДО с активной проблемой {_workerName}: {ex.Message}", stoppingToken);
			}
		}
	}
}
