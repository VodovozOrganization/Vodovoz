using Edo.Problem.Routine.Services.NewEdoTasksResend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Edo;
using Vodovoz.Zabbix.Sender;

namespace Edo.Problem.Routine.Worker
{
	/// <summary>
	/// Периодически повторно запускает просроченные новые задачи ЭДО
	/// </summary>
	public class NewEdoTasksResendWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<NewEdoTasksResendWorker> _logger;
		private readonly IEdoProblemRoutineSettings _settings;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public NewEdoTasksResendWorker(
			ILogger<NewEdoTasksResendWorker> logger,
			IEdoProblemRoutineSettings settings,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));

			if(_settings.NewTasksResendWorkerInterval <= TimeSpan.Zero)
			{
				throw new InvalidOperationException("Интервал работы воркера повторного запуска новых задач должен быть больше нуля");
			}

			if(_settings.NewTasksResendTimeout <= TimeSpan.Zero)
			{
				throw new InvalidOperationException("Таймаут новой задачи ЭДО должен быть больше нуля");
			}

			if(_settings.NewTasksResendBatchSize <= 0)
			{
				throw new InvalidOperationException("Размер партии новых задач ЭДО должен быть больше нуля");
			}
		}

		protected override TimeSpan Interval => _settings.NewTasksResendWorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var resendService = scope.ServiceProvider.GetRequiredService<INewEdoTasksResendService>();
				var maxCreationTime = DateTime.Now.Subtract(_settings.NewTasksResendTimeout);

				try
				{
					await resendService.ResendStaleNewTasks(maxCreationTime, _settings.NewTasksResendBatchSize, stoppingToken);
					await _zabbixSender.SendIsHealthyAsync(nameof(NewEdoTasksResendWorker), stoppingToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка повторного запуска просроченных новых задач ЭДО");
					await _zabbixSender.SendProblemMessageAsync(
						nameof(NewEdoTasksResendWorker),
						ZabixSenderMessageType.Problem,
						$"Ошибка повторного запуска просроченных новых задач ЭДО: {ex.Message}",
						stoppingToken);
				}
			}
		}
	}
}
