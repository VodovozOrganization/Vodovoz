using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using DriverAPI.Services;
using FluentNHibernate.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Tls;
using Renci.SshNet.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace DriverAPI.Workers
{
	public class WakeUpSendCoordinatesNotificationSenderWorker : TimerBackgroundServiceBase
	{
		protected readonly ILogger<WakeUpSendCoordinatesNotificationSenderWorker> _logger;
		private readonly IFCMAPIHelper _fCMAPIHelper;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;

		public WakeUpSendCoordinatesNotificationSenderWorker(
			ILogger<WakeUpSendCoordinatesNotificationSenderWorker> logger,
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

			_logger.LogInformation("Wake up coordinates notification interval: {WakeUpCoordinatesNotificationInterval} seconds", interval);


			if(_wakeUpDriverClientService.Clients.IsEmpty())
			{
				var tokens = employeeData.GetAllPushNotifiableTokens();

				foreach(var token in tokens)
				{
					_wakeUpDriverClientService.Clients.Add(token);
				}
			}

			_logger.LogInformation("Wake up coordinates notification clients count: {WakeUpCoordinatesNotificationClientsCount}", _wakeUpDriverClientService.Clients.Count);
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			_logger.LogInformation("WakeUp Worker tick");
			foreach(var client in _wakeUpDriverClientService.Clients)
			{
				_logger.LogInformation("Sended PushNotification to {FirebaseToken}", client);
				await _fCMAPIHelper.SendWakeUpNotification(client);
			}
		}
	}
}
