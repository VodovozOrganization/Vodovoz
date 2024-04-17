using DriverAPI.Library.V5.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Services
{
	internal sealed class WakeUpDriverClientService : IWakeUpDriverClientService
	{
		private readonly ILogger<WakeUpDriverClientService> _logger;
		private readonly IEmployeeService _employeeService;
		private ConcurrentDictionary<int, string> _clients = new();

		public WakeUpDriverClientService(
			ILogger<WakeUpDriverClientService> logger,
			IEmployeeService employeeService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			var drivers = _employeeService.GetAllPushNotifiableEmployees();

			foreach(var driver in drivers)
			{
				var userApp = driver.DriverAppUser;
				
				if(_clients.TryAdd(driver.Id, userApp.Token))
				{
					_logger.LogTrace(
						"Предзагружен получатель WakeUp-сообщений {DriverId} с токеном {FirebaseToken}",
						driver.Id,
						userApp.Token);
					continue;
				}

				_logger.LogWarning(
					"Не удалось предзагрузить получателя WakeUp-сообщений {DriverId} с токеном {FirebaseToken}",
					driver.Id,
					userApp.Token);
			}

			_logger.LogInformation("Зарегистрировано {WakeUpCoordinatesNotificationClientsCount} клиентов для получения WakeUp-сообщений", Clients.Count);
		}

		public IReadOnlyDictionary<int, string> Clients { get => _clients; }

		public void Subscribe(Employee driver, string token)
		{
			try
			{
				if(_clients.TryAdd(driver.Id, token))
				{
					_logger.LogInformation("Водитель {DriverId} подписан на WakeUp-сообщения с токеном {FirebaseToken}",
						driver.Id,
						token);

					return;
				}

				if(_clients.TryGetValue(driver.Id, out var activeToken)
					&& _clients.TryUpdate(driver.Id, token, activeToken))
				{
					_logger.LogInformation("У водителя уже зарегистрирован токен {FirebaseToken}", activeToken);
					_logger.LogInformation("Водитель {DriverId} переподписан на WakeUp-сообщения с токеном {FirebaseToken}",
						driver.Id,
						token);

					return;
				}

				_logger.LogError("Не удалось подписать водителя {DriverId} на WakeUp-сообщения, токен {FirebaseToken}",
					driver.Id,
					token);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Не удалось подписать водителя {DriverId} на WakeUp-сообщения с токеном {FirebaseToken}",
					driver.Id,
					token);
			}
		}

		public void UnSubscribe(Employee driver)
		{
			var userApp = driver.DriverAppUser;

			try
			{
				if(!_clients.TryGetValue(driver.Id, out var activeToken))
				{
					_logger.LogWarning("Не удалось отписать водителя {DriverId} от WakeUp-сообщений, токен {FirebaseToken}, водитель не подписан на WakeUp-сообщения",
						driver.Id,
						userApp.Token);

					return;
				}

				if(_clients.TryRemove(driver.Id, out activeToken))
				{
					_logger.LogInformation("Водитель {DriverId} отписан от WakeUp-сообщений, аннулированый токен {FirebaseToken}",
						driver.Id,
						activeToken);

					return;
				}

				_logger.LogError("Не удалось отписать водителя {DriverId} от WakeUp-сообщений, токен {FirebaseToken}",
					driver.Id,
					userApp.Token);
			}
			catch(Exception e)
			{
				_logger.LogCritical(e, "Не удалось отписать водителя {DriverId} от WakeUp-сообщений, токен {FirebaseToken}, произошла непредвиденная ошибка",
					driver.Id,
					userApp.Token);
			}
		}

		public void UnSubscribe(string recipientToken)
		{
			var recipientToRemove = _clients.FirstOrDefault(keyValuePair => keyValuePair.Value == recipientToken);
			_clients.TryRemove(recipientToRemove);

			var driver = _employeeService.GetDriverExternalApplicationUserByFirebaseToken(recipientToken);
			_employeeService.DisablePushNotifications(driver);
		}
	}
}
