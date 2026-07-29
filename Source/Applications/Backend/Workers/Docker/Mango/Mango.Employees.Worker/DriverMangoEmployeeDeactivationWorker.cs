using DateTimeHelpers;
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
	/// Воркер деактивации сотрудников Манго. Запускается один раз в сутки в заданное время по МСК
	/// </summary>
	public class DriverMangoEmployeeDeactivationWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<DriverMangoEmployeeDeactivationWorker> _logger;
		private readonly IOptions<DriverMangoEmployeeDeactivationOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public DriverMangoEmployeeDeactivationWorker(
			ILogger<DriverMangoEmployeeDeactivationWorker> logger,
			IOptions<DriverMangoEmployeeDeactivationOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => _options.Value.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();

			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();

			try
			{
				var mangoSettings = scope.ServiceProvider.GetRequiredService<IMangoSettings>();

				if(!mangoSettings.DriverMangoEmployeeDeactivationWorkerEnabled)
				{
					_logger.LogInformation("Работа воркера деактивации сотрудников Манго отключена в настройках");

					await zabbixSender.SendIsHealthyAsync(stoppingToken);

					return;
				}

				var moscowNow = DateTime.UtcNow.ToMoscowDateTime();

				if(IsTimeToRun(moscowNow, mangoSettings))
				{
					_logger.LogInformation("Запуск деактивации сотрудников Манго");

					var deactivationService = scope.ServiceProvider.GetRequiredService<DriverMangoEmployeeDeactivationService>();

					// Деактивируем только номера, выделенные раньше текущих суток по МСК
					await deactivationService.ProcessActiveExtensionNumbersAsync(moscowNow.Date, stoppingToken);

					// Сохраняем дату запуска, чтобы при перезапуске воркера в течение суток
					// деактивация не выполнялась повторно
					mangoSettings.UpdateDriverMangoEmployeeDeactivationLastRunDate(moscowNow.Date);

					_logger.LogInformation("Окончание деактивации сотрудников Манго");
				}

				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при деактивации сотрудников Манго");

				await zabbixSender.SendProblemMessageAsync(
					ZabixSenderMessageType.Problem,
					$"Ошибка при деактивации сотрудников Манго: {ex.Message}",
					stoppingToken);
			}
		}

		private bool IsTimeToRun(DateTime moscowNow, IMangoSettings mangoSettings) =>
			moscowNow.TimeOfDay >= _options.Value.RunTime
			&& mangoSettings.DriverMangoEmployeeDeactivationLastRunDate != moscowNow.Date;
	}
}
