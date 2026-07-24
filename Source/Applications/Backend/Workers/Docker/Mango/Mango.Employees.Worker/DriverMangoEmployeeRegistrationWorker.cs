using Mango.Employees.Library.Options;
using Mango.Employees.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Mango;
using Vodovoz.Zabbix.Sender;

namespace Mango.Employees.Worker
{
	/// <summary>
	/// Воркер регистрации водителей как сотрудников Манго
	/// </summary>
	public class DriverMangoEmployeeRegistrationWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<DriverMangoEmployeeRegistrationWorker> _logger;
		private readonly IOptions<DriverMangoEmployeeRegistrationOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public DriverMangoEmployeeRegistrationWorker(
			ILogger<DriverMangoEmployeeRegistrationWorker> logger,
			IOptions<DriverMangoEmployeeRegistrationOptions> options,
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
				var mangoSettings = scope.ServiceProvider.GetRequiredService<IMangoSettings>();

				if(!mangoSettings.DriverMangoEmployeeRegistrationWorkerEnabled)
				{
					_logger.LogInformation("Работа воркера регистрации сотрудников Манго отключена в настройках");

					await _zabbixSender.SendIsHealthyAsync(stoppingToken);

					return;
				}

				var registrationService = scope.ServiceProvider.GetRequiredService<DriverMangoEmployeeRegistrationService>();

				await registrationService.ProcessNewRequestsAsync(stoppingToken);

				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке заявок на регистрацию сотрудников Манго");

				await _zabbixSender.SendProblemMessageAsync(
					ZabixSenderMessageType.Problem,
					$"Ошибка при обработке заявок на регистрацию сотрудников Манго: {ex.Message}",
					stoppingToken);
			}
		}
	}
}
