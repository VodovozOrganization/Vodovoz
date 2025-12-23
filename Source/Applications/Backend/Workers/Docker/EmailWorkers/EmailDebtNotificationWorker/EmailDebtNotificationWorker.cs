using EmailDebtNotificationWorker.Services;
using System.Text;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Counterparty;

namespace EmailDebtNotificationWorker
{
	public class EmailDebtNotificationWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<EmailDebtNotificationWorker> _logger;
		private readonly IDebtorsSettings _debtorsParameters;
		private readonly IEmailDebtNotificationService _emailSchedulingService;
		private readonly IWorkingDayService _workingDayService;

		private bool _initialized = false;
		protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);

		public EmailDebtNotificationWorker(
			ILogger<EmailDebtNotificationWorker> logger,
			IDebtorsSettings debtorsParameters,
			IEmailDebtNotificationService emailSchedulingService,
			IWorkingDayService workingDayService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_debtorsParameters = debtorsParameters ?? throw new ArgumentNullException(nameof(debtorsParameters));
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
					_logger.LogInformation("Воркер ещё не инициализирован, пропуск цикла");
					return;
				}

				if(!IsEnabled())
				{
					_logger.LogInformation("Рассылка писем отключена настройками, пропуск цикла");
					return;
				}

				if(!CanSendNow())
				{
					_logger.LogDebug("Невозможно отправить сейчас — вне рабочего времени/дня");
					return;
				}

				await _emailSchedulingService.ProcessEmailQueueAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка в воркере рассылки писем");
			}
		}

		private bool CanSendNow()
		{
			var now = DateTime.UtcNow;

			return _workingDayService.IsWorkingDay(now) &&
				   _workingDayService.IsWithinWorkingHours(now);
		}

		private bool IsEnabled() => !_debtorsParameters.DebtNotificationWorkerIsDisabled;

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Запуск воркера рассылки писем...");

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			await base.StartAsync(cancellationToken);

			_initialized = true;
			_logger.LogInformation("Воркер рассылки писем успешно запущен.");
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Остановка воркера рассылки писем...");

			_initialized = false;

			await base.StopAsync(cancellationToken);

			_logger.LogInformation("Воркер рассылки писем успешно остановлен");
		}
	}
}
