﻿using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Services
{
	public sealed class WakeUpDriverClientService : IWakeUpDriverClientService
	{
		private readonly ILogger<WakeUpDriverClientService> _logger;

		private ConcurrentDictionary<int, string> _clients = new();

		public WakeUpDriverClientService(
			ILogger<WakeUpDriverClientService> logger,
			IEmployeeModel employeeModel)
		{
			_logger = logger;

			var drivers = employeeModel.GetAllPushNotifiableEmployees();

			foreach(var driver in drivers)
			{
				if(_clients.TryAdd(driver.Id, driver.AndroidToken))
				{
					_logger.LogTrace("Предзагружен получатель WakeUp-сообщений {DriverId} с токеном {FirebaseToken}", driver.Id, driver.AndroidToken);
					continue;
				}

				_logger.LogWarning("Не удалось предзагрузить получателя WakeUp-сообщений {DriverId} с токеном {FirebaseToken}", driver.Id, driver.AndroidToken);
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
			try
			{
				if(!_clients.TryGetValue(driver.Id, out var activeToken))
				{
					_logger.LogWarning("Не удалось отписать водителя {DriverId} от WakeUp-сообщений, токен {FirebaseToken}, водитель не подписан на WakeUp-сообщения",
						driver.Id,
						driver.AndroidToken);

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
					driver.AndroidToken);
			}
			catch(Exception e)
			{
				_logger.LogCritical(e, "Не удалось отписать водителя {DriverId} от WakeUp-сообщений, токен {FirebaseToken}, произошла непредвиденная ошибка",
					driver.Id,
					driver.AndroidToken);
			}
		}
	}
}
