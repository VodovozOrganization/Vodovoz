using DriverAPI.Library.Helpers;
using DriverAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace DriverAPI.Workers
{
	internal class WakeUpNotificationSenderService : TimerBackgroundServiceBase
	{
		protected readonly ILogger<WakeUpNotificationSenderService> _logger;
		private readonly IFCMAPIHelper _fCMAPIHelper;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;

		public WakeUpNotificationSenderService(
			ILogger<WakeUpNotificationSenderService> logger,
			IConfiguration configuration,
			IFCMAPIHelper fCMAPIHelper,
			IWakeUpDriverClientService wakeUpDriverClientService)
		{
			_logger = logger;
			_fCMAPIHelper = fCMAPIHelper;
			_wakeUpDriverClientService = wakeUpDriverClientService;
			var interval = configuration.GetValue("WakeUpCoordinatesNotificationInterval", 30);
			Interval = TimeSpan.FromSeconds(interval);

			_logger.LogInformation("Интервал отправки WakeUp-сообщений: {WakeUpCoordinatesNotificationInterval} секунд", interval);
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				_logger.LogInformation("Начало цикла отправки WakeUp-сообщений {StartExecutedAt}, сообщений ожидают {WakeUpNotificationsClientsCount} клиентов мобильного приложения",
					DateTime.Now,
					_wakeUpDriverClientService.Clients.Count);

				foreach((var clientId, var clientToken) in _wakeUpDriverClientService.Clients)
				{
					try
					{
						_logger.LogInformation("Попытка отправки WakeUp-сообщения для водителя {DriverId} с токеном {FirebaseToken}", clientId, clientToken);
						await _fCMAPIHelper.SendWakeUpNotification(clientToken);
					}
					catch(FCMException e)
					{
						_logger.LogError(e, "Ошибка отправки WakeUp-сообщения, пропуск цикла {StopExecutedAt}", DateTime.Now);
						break;
					}
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла непредвиденная ошибка");
			}
		}
	}
}
