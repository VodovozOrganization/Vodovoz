using EmailDebtNotificationWorker.Services;
using System.Text;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Zabbix.Sender;

namespace EmailDebtNotificationWorker
{
	public class EmailDebtNotificationWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<EmailDebtNotificationWorker> _logger;
		private readonly IDebtorsSettings _debtorsParameters;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly TimeSpan _interval;

		protected override TimeSpan Interval => _interval;

		public EmailDebtNotificationWorker(
			ILogger<EmailDebtNotificationWorker> logger,
			IDebtorsSettings debtorsParameters,
			IServiceScopeFactory scopeFactory
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_debtorsParameters = debtorsParameters ?? throw new ArgumentNullException(nameof(debtorsParameters));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

			_interval = TimeSpan.FromSeconds(Math.Max(1, _debtorsParameters.DebtNotificationWorkerIntervalSeconds));

			Console.OutputEncoding = Encoding.UTF8;
		}

		protected override async Task DoWork(CancellationToken cancellationToken)
		{
			try
			{
				if(!IsEnabled())
				{
					_logger.LogInformation("Рассылка писем отключена настройками, пропуск цикла");
					return;
				}

				using var scope = _scopeFactory.CreateScope();
				var emailSchedulingService = scope.ServiceProvider.GetRequiredService<IEmailDebtNotificationService>();
				var workingDayService = scope.ServiceProvider.GetRequiredService<IWorkingDayService>();
				var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();

				if(!CanSendNow(workingDayService))
				{
					_logger.LogDebug("Невозможно отправить сейчас — вне рабочего времени/дня");
					return;
				}

				await emailSchedulingService.ScheduleDebtNotificationsAsync(cancellationToken);
				await zabbixSender.SendIsHealthyAsync(cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка в воркере рассылки писем");
			}
		}

		private static bool CanSendNow(IWorkingDayService workingDayService)
		{
			var now = DateTime.Now;

			return workingDayService.IsWorkingDay(now) &&
				   workingDayService.IsWithinWorkingHours(now);
		}

		private bool IsEnabled() => !_debtorsParameters.DebtNotificationWorkerIsDisabled;

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Запуск воркера рассылки писем о задолженности...");

			await base.StartAsync(cancellationToken);

			_logger.LogInformation("Воркер рассылки писем успешно запущен.");
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Остановка воркера рассылки писем о задолженности...");

			await base.StopAsync(cancellationToken);

			_logger.LogInformation("Воркер рассылки писем успешно остановлен");
		}
	}
}
