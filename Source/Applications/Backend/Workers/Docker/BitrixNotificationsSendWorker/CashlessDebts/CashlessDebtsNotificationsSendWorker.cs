using BitrixNotificationsSend.Library.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace BitrixNotificationsSendWorker.CashlessDebts
{
	public class CashlessDebtsNotificationsSendWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<CashlessDebtsNotificationsSendWorker> _logger;
		private readonly IOptions<CashlessDebtsNotificationsSendOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public CashlessDebtsNotificationsSendWorker(
			ILogger<CashlessDebtsNotificationsSendWorker> logger,
			IOptions<CashlessDebtsNotificationsSendOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override TimeSpan Interval => _options.Value.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();

			var zabbixSender = scope.ServiceProvider.GetService<IZabbixSender>();

			_logger.LogInformation("Запуск отправки данных по компаниям с долгом по безналу");

			try
			{

			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки данных по компаниям с долгом по безналу");
			}

			await zabbixSender.SendIsHealthyAsync(stoppingToken);

			_logger.LogInformation("Окончание отправки данных по компаниям с долгом по безналу\"");
		}
	}
}
