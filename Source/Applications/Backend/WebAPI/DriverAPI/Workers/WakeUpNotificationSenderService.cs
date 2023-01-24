using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using DriverAPI.Services;
using FluentNHibernate.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace DriverAPI.Workers
{
	public class WakeUpNotificationSenderService : TimerBackgroundServiceBase
	{
		protected readonly ILogger<WakeUpNotificationSenderService> _logger;
		private readonly IFCMAPIHelper _fCMAPIHelper;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;

		public WakeUpNotificationSenderService(
			ILogger<WakeUpNotificationSenderService> logger,
			IConfiguration configuration,
			IFCMAPIHelper fCMAPIHelper,
			IWakeUpDriverClientService wakeUpDriverClientService,
			IEmployeeModel employeeData)
		{
			_logger = logger;
			_fCMAPIHelper = fCMAPIHelper;
			_wakeUpDriverClientService = wakeUpDriverClientService;
			var interval = configuration.GetValue("WakeUpCoordinatesNotificationInterval", 30);
			Interval = TimeSpan.FromSeconds(interval);

			_logger.LogInformation("Интервал отправки WakeUp-сообщений: {WakeUpCoordinatesNotificationInterval} секунд", interval);


			if(_wakeUpDriverClientService.Clients.IsEmpty())
			{
				var tokens = employeeData.GetAllPushNotifiableTokens();

				foreach(var token in tokens)
				{
					_wakeUpDriverClientService.Clients.Add(token);
				}
			}

			_logger.LogInformation("Зарегистрировано {WakeUpCoordinatesNotificationClientsCount} клиентов для получения WakeUp-сообщений", _wakeUpDriverClientService.Clients.Count);
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Начало цикла отправки WakeUp-сообщений {StartExecutedAt}", DateTime.Now);
			foreach(var client in _wakeUpDriverClientService.Clients)
			{
				try
				{
					_logger.LogInformation("Попытка отправки WakeUp-сообщения на {FirebaseToken}", client);
					await _fCMAPIHelper.SendWakeUpNotification(client);
				}
				catch(FCMException e)
				{
					_logger.LogError(e, "Ошибка отправки WakeUp-сообщения, пропуск цикла {StopExecutedAt}", DateTime.Now);
					break;
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Произошла непредвиденная ошибка");
					break;
				}
			}
		}
	}
}
