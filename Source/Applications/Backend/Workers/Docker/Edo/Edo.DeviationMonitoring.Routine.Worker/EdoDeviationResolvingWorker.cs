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
	/// Воркер снятия отклонений документооборота ЭДО
	/// Работает только с задачами и заявками, по которым есть незакрытое отклонение,
	/// и закрывает те отклонения, которые потеряли актуальность
	/// </summary>
	public class EdoDeviationResolvingWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<EdoDeviationResolvingWorker> _logger;
		private readonly IOptionsMonitor<EdoDeviationMonitoringOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public EdoDeviationResolvingWorker(
			ILogger<EdoDeviationResolvingWorker> logger,
			IOptionsMonitor<EdoDeviationMonitoringOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => _options.CurrentValue.ResolvingWorkerInterval;

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
			var resolvingService = scope.ServiceProvider.GetRequiredService<IEdoDeviationResolvingService>();

			_logger.LogInformation("Запуск снятия отклонений документооборота ЭДО");

			try
			{
				await resolvingService.ResolveAsync(stoppingToken);

				await zabbixSender.SendIsHealthyAsync(nameof(EdoDeviationResolvingWorker), stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при снятии отклонений документооборота ЭДО");

				await zabbixSender.SendProblemMessageAsync(
					nameof(EdoDeviationResolvingWorker),
					ZabixSenderMessageType.Problem,
					$"Ошибка при снятии отклонений документооборота ЭДО: {ex.Message}",
					stoppingToken);
			}
		}
	}
}
