using DriverAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Infrastructure;

namespace DriverAPI.Workers
{
	internal class WakeUpNotificationSenderService : TimerBackgroundServiceBase
	{
		protected readonly ILogger<WakeUpNotificationSenderService> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;

		public WakeUpNotificationSenderService(
			ILogger<WakeUpNotificationSenderService> logger,
			IConfiguration configuration,
			IServiceScopeFactory serviceScopeFactory,
			IWakeUpDriverClientService wakeUpDriverClientService)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_wakeUpDriverClientService = wakeUpDriverClientService ?? throw new ArgumentNullException(nameof(wakeUpDriverClientService));
			var interval = configuration.GetValue("WakeUpCoordinatesNotificationInterval", 30);
			Interval = TimeSpan.FromSeconds(interval);

			_logger.LogInformation("Интервал отправки WakeUp-сообщений: {WakeUpCoordinatesNotificationInterval} секунд", interval);
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				using var scope = _serviceScopeFactory.CreateScope();

				var firebaseCloudMessagingService = scope.ServiceProvider.GetRequiredService<IFirebaseCloudMessagingService>();

				_logger.LogInformation("Начало цикла отправки WakeUp-сообщений {StartExecutedAt}, сообщений ожидают {WakeUpNotificationsClientsCount} клиентов мобильного приложения",
					DateTime.Now,
					_wakeUpDriverClientService.Clients.Count);

				foreach((var clientId, var clientToken) in _wakeUpDriverClientService.Clients)
				{
					try
					{
						_logger.LogInformation("Попытка отправки WakeUp-сообщения для водителя {DriverId} с токеном {FirebaseToken}", clientId, clientToken);

						var sendingResult = await firebaseCloudMessagingService.SendWakeUpMessage(clientToken);

						if(sendingResult.IsFailure && sendingResult.Errors.Contains(Vodovoz.FirebaseCloudMessaging.FirebaseCloudMessagingServiceErrors.Unregistered))
						{
							_wakeUpDriverClientService.UnSubscribe(clientToken);
							_logger.LogWarning("Токен получателя не зарегистрирован, водитель {DriverId} отписан от PUSH-уведомлений", clientId);
							continue;
						}

						if(sendingResult.IsFailure)
						{
							_logger.LogWarning("Произошли следующие ошибки при отправке сообщений: {Errors}", string.Join(", ", sendingResult.Errors.Select(e => e.Message)));
							continue;
						}
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
