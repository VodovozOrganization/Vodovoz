using BitrixNotificationsSend.Library;
using BitrixNotificationsSend.Library.Options;
using BitrixNotificationsSend.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace BitrixNotificationsSendWorker.PlannedOrders
{
	public class PlannedOrdersNotificationsSendWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<PlannedOrdersNotificationsSendWorker> _logger;
		private readonly IOptions<PlannedOrdersNotificationsSendOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private DateTime? _lastSentDate;

		public PlannedOrdersNotificationsSendWorker(
			ILogger<PlannedOrdersNotificationsSendWorker> logger,
			IOptions<PlannedOrdersNotificationsSendOptions> options,
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
			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();

			try
			{
				var moscowNow = MoscowDateTime.Now;

				if(IsInSendTimeInterval(moscowNow) && _lastSentDate != moscowNow.Date)
				{
					_logger.LogInformation("Запуск отправки данных по плановым заказам клиентов");

					var notificationsSendService = scope.ServiceProvider.GetRequiredService<PlannedOrdersNotificationsSendService>();

					var isSent = await notificationsSendService.SendNotifications(stoppingToken);

					if(isSent)
					{
						_lastSentDate = moscowNow.Date;
					}

					_logger.LogInformation("Окончание отправки данных по плановым заказам клиентов");
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки данных по плановым заказам клиентов");
			}

			await zabbixSender.SendIsHealthyAsync(stoppingToken);
		}

		private bool IsInSendTimeInterval(DateTime moscowNow) =>
			moscowNow.TimeOfDay >= _options.Value.SendTimeFrom
			&& moscowNow.TimeOfDay < _options.Value.SendTimeTo;
	}
}
