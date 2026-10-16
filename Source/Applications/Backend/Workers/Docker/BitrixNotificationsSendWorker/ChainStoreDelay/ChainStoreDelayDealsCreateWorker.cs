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

namespace BitrixNotificationsSendWorker.ChainStoreDelay
{
	/// <summary>
	/// Воркер создания сделок по отсрочке сети в Битрикс24
	/// </summary>
	public class ChainStoreDelayDealsCreateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ChainStoreDelayDealsCreateWorker> _logger;
		private readonly IOptions<ChainStoreDelayDealsCreateOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		private DateTime? _lastCollectDate;

		public ChainStoreDelayDealsCreateWorker(
			ILogger<ChainStoreDelayDealsCreateWorker> logger,
			IOptions<ChainStoreDelayDealsCreateOptions> options,
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

				if(!bitrixNotificationsSendSettings.ChainStoreDelayDealsSendEnabled)
				{
					_logger.LogInformation("Работа воркера создания сделок по отсрочке сети отключена в настройках");
					await _zabbixSender.SendIsHealthyAsync(nameof(ChainStoreDelayDealsCreateWorker), stoppingToken);

					return;
				}

				var moscowNow = DateTime.UtcNow.ToMoscowDateTime();

				if(IsWorkingDay(moscowNow) && IsInSendTimeInterval(moscowNow))
				{
					var dealsCreateService = scope.ServiceProvider.GetRequiredService<IChainStoreDelayDealsCreateService>();

					if(_lastCollectDate != moscowNow.Date)
					{
						_logger.LogInformation("Запуск сбора данных по отсрочке сети");

						await dealsCreateService.CollectDeals(stoppingToken);

						_lastCollectDate = moscowNow.Date;

						_logger.LogInformation("Окончание сбора данных по отсрочке сети");
					}

					await dealsCreateService.SendNotCreatedDeals(stoppingToken);
				}

				await _zabbixSender.SendIsHealthyAsync(nameof(ChainStoreDelayDealsCreateWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки данных по отсрочке сети");

				await _zabbixSender.SendProblemMessageAsync(
					nameof(ChainStoreDelayDealsCreateWorker),
					ZabixSenderMessageType.Problem,
					$"Ошибка отправки данных по отсрочке сети: {ex.Message}",
					stoppingToken);
			}
		}

		/// <summary>
		/// Рабочий день: не суббота и не воскресенье. Праздники не учитываются (Q12)
		/// </summary>
		private static bool IsWorkingDay(DateTime moscowNow) =>
			moscowNow.DayOfWeek != DayOfWeek.Saturday && moscowNow.DayOfWeek != DayOfWeek.Sunday;

		private bool IsInSendTimeInterval(DateTime moscowNow) =>
			moscowNow.TimeOfDay >= _options.Value.SendTimeFrom
			&& moscowNow.TimeOfDay < _options.Value.SendTimeTo;
	}
}
