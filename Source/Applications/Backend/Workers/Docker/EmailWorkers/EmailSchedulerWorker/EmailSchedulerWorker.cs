using EmailSchedulerWorker.Services;
using QS.DomainModel.UoW;
using System.Text;
using Vodovoz.Infrastructure;
using static EmailSchedulerWorker.Services.EmailSchedulingService;

namespace EmailSchedulerWorker
{
	public class EmailSchedulerWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<EmailSchedulerWorker> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IEmailSchedulingService _emailSchedulingService;
		private readonly IWorkingDayService _workingDayService;

		private bool _initialized = false;
		private int _instanceId;
		protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(30);

		public EmailSchedulerWorker(
			ILogger<EmailSchedulerWorker> logger,
			IUnitOfWork uow,
			IEmailSchedulingService emailSchedulingService,
			IWorkingDayService workingDayService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_emailSchedulingService = emailSchedulingService ?? throw new ArgumentNullException(nameof(emailSchedulingService));
			_workingDayService = workingDayService ?? throw new ArgumentNullException(nameof(workingDayService));

			Console.OutputEncoding = Encoding.UTF8;
		}

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				if(!_initialized)
				{
					_logger.LogInformation("Worker not initialized yet, skipping cycle");
					return;
				}

				if(!CanSendNow())
				{
					_logger.LogDebug("Cannot send now - outside working hours/day");
					return;
				}

				// 3. Обработка очереди email
				await _emailSchedulingService.ProcessEmailQueueAsync(stoppingToken);

				// 4. Логирование статистики (раз в 10 минут)
				await LogStatisticsPeriodicallyAsync();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error in Email Scheduler Worker");
			}
		}

		private bool CanSendNow()
		{
			var now = DateTime.UtcNow;

			return _workingDayService.IsWorkingDay(now) &&
				   _workingDayService.IsWithinWorkingHours(now);
		}

		private async Task LogStatisticsPeriodicallyAsync()
		{
			if(DateTime.UtcNow.Minute % 10 != 0)
			{
				return;
			}

			try
			{
				var pendingCount = await _emailSchedulingService.GetPendingEmailsCountAsync();

				_logger.LogInformation(
					"Email Scheduler Statistics:\n" +
					"  • Pending emails: {PendingCount}\n" +
					"  • Working hours now: {IsWorkingHours}\n" +
					"  • Working day today: {IsWorkingDay}\n",
					pendingCount,
					_workingDayService.IsWithinWorkingHours(DateTime.UtcNow),
					_workingDayService.IsWorkingDay(DateTime.UtcNow));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error logging statistics");
			}
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting Email Scheduler Worker...");

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			_instanceId = Convert.ToInt32(_uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			await base.StartAsync(cancellationToken);

			_initialized = true;
			_logger.LogInformation(
				"Email Scheduler Worker started successfully. Instance ID: {InstanceId}",
				_instanceId);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping Email Scheduler Worker...");

			_initialized = false;

			await base.StopAsync(cancellationToken);

			_logger.LogInformation("Email Scheduler Worker stopped gracefully");
		}
	}
}
