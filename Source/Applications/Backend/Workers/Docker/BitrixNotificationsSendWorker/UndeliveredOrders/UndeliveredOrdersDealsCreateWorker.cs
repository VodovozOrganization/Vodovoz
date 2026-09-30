using BitrixNotificationsSend.Library.Options;
using BitrixNotificationsSend.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Notifications;
using Vodovoz.Zabbix.Sender;

namespace BitrixNotificationsSendWorker.UndeliveredOrders
{
	/// <summary>
	/// Воркер создания сделок по недовозам в Битрикс24.
	/// </summary>
	public class UndeliveredOrdersDealsCreateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<UndeliveredOrdersDealsCreateWorker> _logger;
		private readonly IOptions<UndeliveredOrdersDealsCreateOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		private DateTime? _lastCollectTime;

		public UndeliveredOrdersDealsCreateWorker(
			ILogger<UndeliveredOrdersDealsCreateWorker> logger,
			IOptions<UndeliveredOrdersDealsCreateOptions> options,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override TimeSpan Interval => _options.Value.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();

			try
			{
				var bitrixNotificationsSendSettings = scope.ServiceProvider.GetRequiredService<IBitrixNotificationsSendSettings>();

				if(!bitrixNotificationsSendSettings.UndeliveredOrdersSendEnabled)
				{
					_logger.LogInformation("Работа воркера отправки недовозов в Битрикс24 отключена в настройках");
					await _zabbixSender.SendIsHealthyAsync(nameof(UndeliveredOrdersDealsCreateWorker), stoppingToken);

					return;
				}

				var dealsCreateService = scope.ServiceProvider.GetRequiredService<IUndeliveredOrdersDealsCreateService>();
				var minLastEditedTime = _lastCollectTime ?? _options.Value.MinLastEditedTime;
				var collectStartedAt = DateTime.Now;

				_logger.LogInformation("Запуск сбора данных по недовозам");

				await dealsCreateService.CollectUndeliveredOrders(minLastEditedTime, stoppingToken);

				_lastCollectTime = collectStartedAt;

				_logger.LogInformation("Окончание сбора данных по недовозам");

				await dealsCreateService.SendNotCreatedDeals(stoppingToken);
				await dealsCreateService.SendNotActualDeals(stoppingToken);

				await _zabbixSender.SendIsHealthyAsync(nameof(UndeliveredOrdersDealsCreateWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки данных по недовозам");

				await _zabbixSender.SendProblemMessageAsync(
					nameof(UndeliveredOrdersDealsCreateWorker),
					ZabixSenderMessageType.Problem,
					$"Ошибка отправки данных по недовозам: {ex.Message}",
					stoppingToken);
			}
		}
	}
}
