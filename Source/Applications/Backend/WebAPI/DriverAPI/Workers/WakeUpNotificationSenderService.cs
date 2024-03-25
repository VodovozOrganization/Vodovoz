using DriverAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Infrastructure;

namespace DriverAPI.Workers
{
	internal class WakeUpNotificationSenderService : TimerBackgroundServiceBase
	{
		protected readonly ILogger<WakeUpNotificationSenderService> _logger;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;
		private readonly IFirebaseCloudMessagingService _firebaseCloudMessagingService;

		public WakeUpNotificationSenderService(
			ILogger<WakeUpNotificationSenderService> logger,
			IConfiguration configuration,
			IWakeUpDriverClientService wakeUpDriverClientService,
			IFirebaseCloudMessagingService firebaseCloudMessagingService)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_wakeUpDriverClientService = wakeUpDriverClientService ?? throw new ArgumentNullException(nameof(wakeUpDriverClientService));
			_firebaseCloudMessagingService = firebaseCloudMessagingService ?? throw new ArgumentNullException(nameof(firebaseCloudMessagingService));
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
						await _firebaseCloudMessagingService.SendWakeUpMessage(clientToken);
					}
					catch(Exception e)
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
