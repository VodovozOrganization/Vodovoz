using BitrixNotificationsSend.Library.Options;
using BitrixNotificationsSend.Library.Services;
using DateTimeHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Notifications;
using Vodovoz.Zabbix.Sender;

namespace BitrixNotificationsSendWorker.PlannedOrders
{
	/// <summary>
	/// Воркер создания сделок по плановым заказам клиентов в Битрикс24
	/// </summary>
	public class PlannedOrdersDealsCreateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<PlannedOrdersDealsCreateWorker> _logger;
		private readonly IOptions<PlannedOrdersDealsCreateOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		private DateTime? _lastCollectDate;

		public PlannedOrdersDealsCreateWorker(
			ILogger<PlannedOrdersDealsCreateWorker> logger,
			IOptions<PlannedOrdersDealsCreateOptions> options,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override TimeSpan Interval => _options.Value.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();

			try
			{
				var bitrixNotificationsSendSettings = scope.ServiceProvider.GetRequiredService<IBitrixNotificationsSendSettings>();

				if(!bitrixNotificationsSendSettings.PlannedOrdersNotificationsSendEnabled)
				{
					_logger.LogInformation("Работа воркера отправки уведомлений по плановым заказам отключена в настройках");
					await _zabbixSender.SendIsHealthyAsync(nameof(PlannedOrdersDealsCreateWorker), stoppingToken);

					return;
				}

				var moscowNow = DateTime.UtcNow.ToMoscowDateTime();

				if(IsInSendTimeInterval(moscowNow))
				{
					var dealsCreateService = scope.ServiceProvider.GetRequiredService<PlannedOrdersDealsCreateService>();

					if(_lastCollectDate != moscowNow.Date)
					{
						_logger.LogInformation("Запуск сбора данных по плановым заказам клиентов");

						await dealsCreateService.CollectPlannedOrders(stoppingToken);

						_lastCollectDate = moscowNow.Date;

						_logger.LogInformation("Окончание сбора данных по плановым заказам клиентов");
					}

					await dealsCreateService.SendNotCreatedDeals(stoppingToken);

					_logger.LogInformation("Запуск поиска заказов, созданных клиентами по плановым заказам");

					await dealsCreateService.CollectCreatedOrders(stoppingToken);

					_logger.LogInformation("Окончание поиска заказов, созданных клиентами по плановым заказам");

					await dealsCreateService.SendDealsUpdates(stoppingToken);
				}

				await _zabbixSender.SendIsHealthyAsync(nameof(PlannedOrdersDealsCreateWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки данных по плановым заказам клиентов");

				await _zabbixSender.SendProblemMessageAsync(
					nameof(PlannedOrdersDealsCreateWorker),
					ZabixSenderMessageType.Problem,
					$"Ошибка отправки данных по плановым заказам клиентов: {ex.Message}",
					stoppingToken);
			}
		}

		private bool IsInSendTimeInterval(DateTime moscowNow) =>
			moscowNow.TimeOfDay >= _options.Value.SendTimeFrom
			&& moscowNow.TimeOfDay < _options.Value.SendTimeTo;
	}
}
