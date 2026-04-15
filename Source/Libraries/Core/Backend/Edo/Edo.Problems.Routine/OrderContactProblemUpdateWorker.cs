using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Application.Problems.Services;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace Edo.Problems.Routine
{
	public class OrderContactProblemUpdateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<OrderContactProblemUpdateWorker> _logger;
		private readonly IZabbixSender _zabbixSender;
		private readonly IOrderContactProblemUpdateService _contactProblemUpdateService;

		private const int _intervalMinutes = 60;

		public OrderContactProblemUpdateWorker(
			ILogger<OrderContactProblemUpdateWorker> logger,
			IZabbixSender zabbixSender,
			IOrderContactProblemUpdateService contactProblemUpdateService)
		{
			_logger = logger ?? throw new  ArgumentNullException(nameof(logger));
			_zabbixSender = zabbixSender ?? throw new  ArgumentNullException(nameof(zabbixSender));
			_contactProblemUpdateService = contactProblemUpdateService ?? throw new  ArgumentNullException(nameof(contactProblemUpdateService));

			Interval = TimeSpan.FromMinutes(_intervalMinutes);
		}
		
		protected override TimeSpan Interval { get; }
		
		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
				
				await _contactProblemUpdateService.UpdateEdoTaskWithContactProblemAsync(stoppingToken);
			}
			catch(Exception e)
			{
				await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem,
					$"Ошибка при обработке заказов в: {DateTimeOffset.Now}\nСообщение: " + e.Message,
					stoppingToken);
				
				_logger.LogError(
					e,
					"Ошибка при обработке заказов в: {ErrorDateTime}",
					DateTimeOffset.Now);
			}
			
			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayTime}' мин. перед следующим запуском", nameof(OrderContactProblemUpdateWorker), _intervalMinutes);
		}
		
		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {StartTime}",
				nameof(OrderContactProblemUpdateWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(OrderContactProblemUpdateWorker),
				DateTimeOffset.Now);

			base.OnStopService();
		}
	}
}
