using Edo.DeviationMonitoring.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Edo;
using Vodovoz.Zabbix.Sender;

namespace Edo.DeviationMonitoring.Routine.Worker
{
	/// <summary>
	/// Воркер регистрации отклонений документооборота ЭДО
	/// Обходит незавершенные задачи и заявки без задач и заводит новые отклонения
	/// </summary>
	public class EdoDeviationRegistrationWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<EdoDeviationRegistrationWorker> _logger;
		private readonly IOptionsMonitor<EdoDeviationMonitoringOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public EdoDeviationRegistrationWorker(
			ILogger<EdoDeviationRegistrationWorker> logger,
			IOptionsMonitor<EdoDeviationMonitoringOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => _options.CurrentValue.RegistrationWorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();

			var edoDeviationSettings = scope.ServiceProvider.GetRequiredService<IEdoDeviationSettings>();

			if(!edoDeviationSettings.IsEnabled)
			{
				_logger.LogInformation("Мониторинг отклонений документооборота ЭДО выключен настройкой");

				return;
			}

			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
			var registrationService = scope.ServiceProvider.GetRequiredService<IEdoDeviationRegistrationService>();

			_logger.LogInformation("Запуск регистрации отклонений документооборота ЭДО");

			try
			{
				await registrationService.RegisterAsync(stoppingToken);

				await zabbixSender.SendIsHealthyAsync(nameof(EdoDeviationRegistrationWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при регистрации отклонений документооборота ЭДО");

				await zabbixSender.SendProblemMessageAsync(
					nameof(EdoDeviationRegistrationWorker),
					ZabixSenderMessageType.Problem,
					$"Ошибка при регистрации отклонений документооборота ЭДО: {ex.Message}",
					stoppingToken);
			}
		}
	}
}
