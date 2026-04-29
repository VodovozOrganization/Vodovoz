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

		public EmailClaimLettersWorker(
			ILogger<EmailClaimLettersWorker> logger,
			IOptionsMonitor<EmailClaimLettersOptions> emailClaimLettersOptions,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_emailClaimLettersOptions = emailClaimLettersOptions ?? throw new ArgumentNullException(nameof(emailClaimLettersOptions));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => _emailClaimLettersOptions.CurrentValue.OverdueDebtorDebtInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
			var emailClaimLettersService = scope.ServiceProvider.GetRequiredService<IEmailClaimLettersService>();

			_logger.LogInformation("Отправляем письма с претензиями");

			try
			{
				await emailClaimLettersService.SendClaimLetters(stoppingToken);
				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке писем с претензиями");
			}

			_logger.LogInformation("Завершение отправки писем с претензиями");
		}
	}
}
