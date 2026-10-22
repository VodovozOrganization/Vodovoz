using DateTimeHelpers;
using EmailDebtNotificationWorker.Options;
using EmailDebtNotificationWorker.Services.ChainStoreDelayLetters;
using EmailDebtNotificationWorker.Services.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Zabbix.Sender;

namespace EmailDebtNotificationWorker
{
	/// <summary>
	/// Воркер рассылки писем по заказам сетей и тендерных клиентов (напоминание об оплате)
	/// </summary>
	public class ChainStoreDelayLettersWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ChainStoreDelayLettersWorker> _logger;
		private readonly IOptions<ChainStoreDelayLettersOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		private DateTime? _lastCollectDate;

		public ChainStoreDelayLettersWorker(
			ILogger<ChainStoreDelayLettersWorker> logger,
			IOptions<ChainStoreDelayLettersOptions> options,
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
				var settings = scope.ServiceProvider.GetRequiredService<IChainStoreDelayLettersSettings>();

				if(!settings.SendEnabled)
				{
					_logger.LogInformation("Работа воркера писем по заказам сетей и тендеров отключена в настройках");
					await _zabbixSender.SendIsHealthyAsync(nameof(ChainStoreDelayLettersWorker), stoppingToken);

					return;
				}

				var moscowNow = DateTime.UtcNow.ToMoscowDateTime();
				var workingDayService = scope.ServiceProvider.GetRequiredService<IWorkingDayService>();

				if(workingDayService.IsWorkingDay(DateTime.Now) && IsInSendTimeInterval(moscowNow))
				{
					var lettersService = scope.ServiceProvider.GetRequiredService<IChainStoreDelayLettersService>();

					if(_lastCollectDate != moscowNow.Date)
					{
						_logger.LogInformation("Запуск отбора заказов для писем по заказам сетей и тендеров");

						await lettersService.CollectLetters(stoppingToken);

						_lastCollectDate = moscowNow.Date;
					}

					await lettersService.SendLetters(stoppingToken);
				}

				await _zabbixSender.SendIsHealthyAsync(nameof(ChainStoreDelayLettersWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки писем по заказам сетей и тендеров");

				await _zabbixSender.SendProblemMessageAsync(
					nameof(ChainStoreDelayLettersWorker),
					ZabixSenderMessageType.Problem,
					$"Ошибка отправки писем по заказам сетей и тендеров: {ex.Message}",
					stoppingToken);
			}
		}

		private bool IsInSendTimeInterval(DateTime moscowNow) =>
			moscowNow.TimeOfDay >= _options.Value.SendTimeFrom
			&& moscowNow.TimeOfDay < _options.Value.SendTimeTo;
	}
}
