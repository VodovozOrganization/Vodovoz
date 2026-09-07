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
	/// Воркер снятия отклонений документооборота ЭДО.
	/// Работает только с задачами и заявками, по которым есть незакрытое отклонение,
	/// и закрывает те отклонения, которые потеряли актуальность
	/// </summary>
	public class EdoDeviationResolvingWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<EdoDeviationResolvingWorker> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private TimeSpan _interval;

		public EdoDeviationResolvingWorker(
			ILogger<EdoDeviationResolvingWorker> logger,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		/// <summary>
		/// Интервал перечитывается перед каждым ожиданием: настройка правится в базе,
		/// и менять ее не должно требовать перезапуска сервиса
		/// </summary>
		protected override TimeSpan Interval => GetInterval();

		/// <summary>
		/// Читает интервал из настроек. Если прочитать не удалось, а прежнее значение
		/// уже известно, воркер продолжает работать с ним: разовый сбой чтения настройки
		/// не должен останавливать цикл
		/// </summary>
		private TimeSpan GetInterval()
		{
			try
			{
				using(var scope = _serviceScopeFactory.CreateScope())
				{
					_interval = scope.ServiceProvider
						.GetRequiredService<IOptionsSnapshot<EdoDeviationMonitoringOptions>>()
						.Value.ResolvingWorkerInterval;
				}
			}
			catch(Exception ex) when(_interval != default)
			{
				_logger.LogError(ex,
					"Не удалось прочитать интервал работы воркера снятия отклонений ЭДО, "
					+ "продолжаем с прежним значением {Interval}",
					_interval);
			}

			return _interval;
		}

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
