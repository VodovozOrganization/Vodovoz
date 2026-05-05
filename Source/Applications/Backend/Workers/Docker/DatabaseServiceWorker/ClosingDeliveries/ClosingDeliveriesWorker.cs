using DatabaseServiceWorker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Zabbix.Sender;
using VodovozBusiness.Services.Orders;

namespace DatabaseServiceWorker.ClosingDeliveries
{
	public class ClosingDeliveriesWorker : BackgroundService
	{
		private readonly ILogger<ClosingDeliveriesWorker> _logger;
		private readonly IOptions<ClosingDeliveriesOptions> _options;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public ClosingDeliveriesWorker(
			ILogger<ClosingDeliveriesWorker> logger,
			IOptions<ClosingDeliveriesOptions> options,
			IServiceScopeFactory scopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Воркер закрытия поставок контрагентам запущен");

			while(!stoppingToken.IsCancellationRequested)
			{
				await DelayToNextDay(stoppingToken);

				await RunClosingCycleAsync(stoppingToken);
			}
		}

		private async Task RunClosingCycleAsync(CancellationToken stoppingToken)
		{
			try
			{
				_logger.LogInformation("Начинаем закрытие поставок контрагентам");

				using var scope = _scopeFactory.CreateScope();

				var closingDeliveriesService = scope.ServiceProvider.GetRequiredService<IClosingDeliveriesService>();
				var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();

				using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot(nameof(ClosingDeliveriesWorker));

				await closingDeliveriesService.CheckAndCloseDeliveriesAsync(unitOfWork, cancellationToken: stoppingToken);

				await unitOfWork.CommitAsync(stoppingToken);

				_logger.LogInformation("Закрытие поставок контрагентам завершено");

				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выполнении закрытия поставок контрагентам");
				await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, ex.Message, stoppingToken);
			}
		}

		private async Task DelayToNextDay(CancellationToken token)
		{
			var now = DateTime.Now;
			var startHour = _options.Value.StartHour;

			var nextRun = now.Date.AddHours(startHour);

			if(now >= nextRun)
			{
				nextRun = nextRun.AddDays(1);
			}

			var delay = nextRun - now;

			await Task.Delay(delay, token);
		}
	}
}
