using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services.CodeDuplicatedProblem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
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
		private readonly IZabbixSender _zabbixSender;
		private const string _problemName = "Дубликат кода";

		public CodeDuplicatedProblemWorker(
			ILogger<CodeDuplicatedProblemWorker> logger,
			IOptions<CodeDuplicatedProblemWorkerOptions> options,
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

			var codeDuplicatedProblemService = scope.ServiceProvider.GetRequiredService<ICodeDuplicatedProblemService>();
			var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();

			_logger.LogInformation($"Запуск обработки задач ЭДО с активной проблемой {_problemName}");

			try
			{
				var minEdoTaskCreationTime = DateTime.Today - _options.Value.ProblemTimeout;

				using(var unitOfWork = unitOfWorkFactory.CreateWithoutRoot(nameof(CodeDuplicatedProblemWorker)))
				{
					await codeDuplicatedProblemService.ProcessProblemTasksAsync(unitOfWork, minEdoTaskCreationTime, stoppingToken);
				}

				_logger.LogInformation($"Обработка задач ЭДО с активной проблемой {_problemName} успешно завершена");

				await _zabbixSender.SendIsHealthyAsync(nameof(CodeDuplicatedProblemWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при обработке задач ЭДО с активной проблемой {_problemName}");

				await _zabbixSender.SendProblemMessageAsync(nameof(CodeDuplicatedProblemWorker), ZabixSenderMessageType.Problem,
					$"Ошибка при обработке задач ЭДО с активной проблемой {_problemName}: {ex.Message}", stoppingToken);
			}
		}
	}
}
