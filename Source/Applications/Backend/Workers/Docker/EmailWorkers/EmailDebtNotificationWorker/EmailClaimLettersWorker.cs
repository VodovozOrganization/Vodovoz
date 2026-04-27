using EmailDebtNotificationWorker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public EmailClaimLettersWorker(
			ILogger<EmailClaimLettersWorker> logger,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => TimeSpan.FromSeconds(60);

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
