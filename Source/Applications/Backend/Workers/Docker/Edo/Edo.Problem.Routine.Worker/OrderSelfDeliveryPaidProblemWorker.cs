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
	public class OrderSelfDeliveryPaidProblemWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<OrderSelfDeliveryPaidProblemWorker> _logger;
		private readonly IOptions<OrderSelfDeliveryPaidProblemWorkerOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public OrderSelfDeliveryPaidProblemWorker(
			ILogger<OrderSelfDeliveryPaidProblemWorker> logger,
			IOptions<OrderSelfDeliveryPaidProblemWorkerOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override TimeSpan Interval => _options.Value.WorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
				var orderSelfDeliveryPaidProblemService = scope.ServiceProvider.GetRequiredService<OrderSelfDeliveryPaidProblemService>();

				_logger.LogInformation("Запуск обработки задач ЭДО с активной проблемой оплаты самовывоза");

				try
				{
					await orderSelfDeliveryPaidProblemService.ProcessProblemTasks(stoppingToken);

					_logger.LogInformation("Обработка задач ЭДО с активной проблемой оплаты самовывоза успешно завершена");
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке задач ЭДО с активной проблемой оплаты самовывоза");
				}

				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
		}
	}
}
