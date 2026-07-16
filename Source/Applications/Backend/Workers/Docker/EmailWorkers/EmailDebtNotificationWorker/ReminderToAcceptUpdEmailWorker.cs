using EmailDebtNotificationWorker.Options;
using EmailDebtNotificationWorker.Services.ReminderToAcceptUpd;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure.Scheduling;
using Vodovoz.Zabbix.Sender;

namespace EmailDebtNotificationWorker
{
	public class ReminderToAcceptUpdEmailWorker : BackgroundService
	{
		private readonly ILogger<ReminderToAcceptUpdEmailWorker> _logger;
		private readonly IOptions<ReminderToAcceptUpdEmailOptions> _options;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IZabbixSender _zabbixSender;
		private readonly IDailyScheduler _dailyScheduler;
		private const string _workerName = "Воркер отправки писем с напоминанием о необходимости принятия УПД";

		public ReminderToAcceptUpdEmailWorker(
			ILogger<ReminderToAcceptUpdEmailWorker> logger,
			IOptions<ReminderToAcceptUpdEmailOptions> options,
			IServiceScopeFactory scopeFactory,
			IZabbixSender zabbixSender,
			IDailyScheduler dailyScheduler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_dailyScheduler = dailyScheduler ?? throw new ArgumentNullException(nameof(dailyScheduler));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("{WorkerName} запущен", _workerName);

			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await _dailyScheduler.DelayUntilNextOccurrenceAsync(
						_options.Value.StartTime,
						_workerName,
						stoppingToken);

					await RunCycleAsync(stoppingToken);

					await _zabbixSender.SendIsHealthyAsync(stoppingToken);
				}
				catch(OperationCanceledException)
				{
					_logger.LogInformation("{WorkerName} получил сигнал остановки", _workerName);
					break;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "{WorkerName} произошла ошибка в основном цикле", _workerName);
					await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, ex.Message, stoppingToken);
					await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
				}
			}

			_logger.LogInformation("{WorkerName} остановлен", _workerName);
		}

		private async Task RunCycleAsync(CancellationToken stoppingToken)
		{
			using var scope = _scopeFactory.CreateScope();
			var reminderToAcceptUpdService = scope.ServiceProvider.GetRequiredService<IReminderToAcceptUpdService>();
			var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();

			using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot(nameof(ReminderToAcceptUpdEmailWorker));

			await reminderToAcceptUpdService.RemindToAcceptUpd(unitOfWork, _options.Value.TimeoutDays, stoppingToken);
		}
	}
}
