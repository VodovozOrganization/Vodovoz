using EmailDebtNotificationWorker.Options;
using EmailDebtNotificationWorker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace EmailDebtNotificationWorker
{
	public class EmailClaimLettersWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<EmailClaimLettersWorker> _logger;
		private readonly IOptionsMonitor<EmailClaimLettersOptions> _emailClaimLettersOptions;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public EmailClaimLettersWorker(
			ILogger<EmailClaimLettersWorker> logger,
			IOptionsMonitor<EmailClaimLettersOptions> emailClaimLettersOptions,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_emailClaimLettersOptions = emailClaimLettersOptions ?? throw new ArgumentNullException(nameof(emailClaimLettersOptions));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override TimeSpan Interval => _emailClaimLettersOptions.CurrentValue.WorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var emailClaimLettersService = scope.ServiceProvider.GetRequiredService<IEmailClaimLettersService>();
			var workingDayService = scope.ServiceProvider.GetRequiredService<IWorkingDayService>();

			if(!CanSendNow(workingDayService))
			{
				_logger.LogInformation("Сейчас нерабочее время, пропускаем отправку писем с претензиями");
				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
				return;
			}

			_logger.LogInformation("Отправляем письма с претензиями");

			try
			{
				await emailClaimLettersService.SendClaimLetters(stoppingToken);
				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке писем с претензиями");
			}

			_logger.LogInformation("Завершение отправки писем с претензиями");
		}

		private static bool CanSendNow(IWorkingDayService workingDayService)
		{
			var now = DateTime.Now;

			return workingDayService.IsWithinWorkingHours(now) && workingDayService.IsWorkingDay(now);
		}
	}
}
