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

namespace BitrixNotificationsSendWorker.LastServiceOrders
{
	/// <summary>
	/// Воркер создания сделок по последним сервисным заказам клиентов в Битрикс24
	/// </summary>
	public class LastServiceOrdersDealsCreateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<LastServiceOrdersDealsCreateWorker> _logger;
		private readonly IOptions<LastServiceOrdersDealsCreateOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		private DateTime? _lastCollectDate;

		public LastServiceOrdersDealsCreateWorker(
			ILogger<LastServiceOrdersDealsCreateWorker> logger,
			IOptions<LastServiceOrdersDealsCreateOptions> options,
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

				if(!bitrixNotificationsSendSettings.LastServiceOrdersNotificationsSendEnabled)
				{
					_logger.LogInformation("Работа воркера отправки уведомлений по последним сервисным заказам отключена в настройках");
					await _zabbixSender.SendIsHealthyAsync(stoppingToken);

					return;
				}

				var moscowNow = DateTime.UtcNow.ToMoscowDateTime();

				if(IsInSendTimeInterval(moscowNow))
				{
					var dealsCreateService = scope.ServiceProvider.GetRequiredService<LastServiceOrdersDealsCreateService>();

					if(_lastCollectDate != moscowNow.Date)
					{
						_logger.LogInformation("Запуск сбора данных по последним сервисным заказам клиентов");

						await dealsCreateService.CollectLastServiceOrders(stoppingToken);

						_lastCollectDate = moscowNow.Date;

						_logger.LogInformation("Окончание сбора данных по последним сервисным заказам клиентов");
					}

					await dealsCreateService.SendNotCreatedDeals(stoppingToken);
				}

				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки данных по последним сервисным заказам клиентов");

				await _zabbixSender.SendProblemMessageAsync(
					ZabixSenderMessageType.Problem,
					$"Ошибка отправки данных по последним сервисным заказам клиентов: {ex.Message}",
					stoppingToken);
			}
		}

		private bool IsInSendTimeInterval(DateTime moscowNow) =>
			moscowNow.TimeOfDay >= _options.Value.SendTimeFrom
			&& moscowNow.TimeOfDay < _options.Value.SendTimeTo;
	}
}
