using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace Edo.Problem.Routine.Worker
{
	public class OrderStatusProblemWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<OrderStatusProblemWorker> _logger;
		private readonly IOptions<OrderStatusProblemWorkerOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public OrderStatusProblemWorker(
			ILogger<OrderStatusProblemWorker> logger,
			IOptions<OrderStatusProblemWorkerOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override TimeSpan Interval => _options.Value.WorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();

			var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
			var orderStatusProblemService = scope.ServiceProvider.GetRequiredService<OrderStatusProblemService>();

			_logger.LogInformation("Запуск обработки задач ЭДО с активной проблемой статуса заказа");

			try
			{
				await orderStatusProblemService.ProcessProblemTasks(stoppingToken);

				_logger.LogInformation("Обработка задач ЭДО с активной проблемой статуса заказа успешно завершена");

				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке задач ЭДО с активной проблемой статуса заказа");

				await zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem,
					$"Ошибка при обработке задач ЭДО с активной проблемой статуса заказа: {ex.Message}", stoppingToken);
			}			
		}
	}
}
