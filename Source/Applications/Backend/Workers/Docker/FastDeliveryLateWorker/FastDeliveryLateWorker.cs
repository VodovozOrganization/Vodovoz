using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace FastDeliveryLateWorker
{
	public class FastDeliveryLateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<FastDeliveryLateWorker> _logger;
		private readonly IOptions<FastDeliveryLateOptions> _options;
		private bool _workInProgress;

		public FastDeliveryLateWorker(ILogger<FastDeliveryLateWorker> logger, IOptions<FastDeliveryLateOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));			
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {StartTime}",
				nameof(FastDeliveryLateWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(FastDeliveryLateWorker),
				DateTimeOffset.Now);

			base.OnStopService();
		}

		protected override TimeSpan Interval => _options.Value.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(_workInProgress)
			{
				return;
			}

			_workInProgress = true;

			try
			{
				//ToDo Repository and Write
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при при обновлении км до ТО {ErrorDateTime}",
					DateTimeOffset.Now);
			}
			finally
			{
				_workInProgress = false;
			}

			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayTime}' перед следующим запуском", nameof(FastDeliveryLateWorker), Interval);

			await Task.CompletedTask;
		}

	}
}
