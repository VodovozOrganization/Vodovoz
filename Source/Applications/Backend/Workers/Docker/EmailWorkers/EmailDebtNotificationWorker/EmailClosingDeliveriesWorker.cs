using EmailDebtNotificationWorker.Options;
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
using System.Linq;
using EmailDebtNotificationWorker.Services.ClosingDeliveries;
using QS.Services;

namespace EmailDebtNotificationWorker
{
	public class EmailClosingDeliveriesWorker : BackgroundService
	{
		private readonly ILogger<EmailClosingDeliveriesWorker> _logger;
		private readonly IOptions<EmailClosingDeliveriesOptions> _options;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public EmailClosingDeliveriesWorker(
			ILogger<EmailClosingDeliveriesWorker> logger,
			IOptions<EmailClosingDeliveriesOptions> options,
			IServiceScopeFactory scopeFactory,
			IZabbixSender zabbixSender,
			IUserService userService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Воркер закрытия поставок и уведомления контрагентов запущен");

			while(!stoppingToken.IsCancellationRequested)
			{
				await DelayToNextDay(stoppingToken);

				await RunClosingCycleAsync(stoppingToken);							
			}

			_logger.LogInformation("Воркер закрытия поставок и отправки почты  контрагентам остановлен");
		}

		private async Task RunClosingCycleAsync(CancellationToken stoppingToken)
		{
			try
			{
				_logger.LogInformation("Начинаем закрытие поставок и отправку почты контрагентам");

				using var scope = _scopeFactory.CreateScope();

				var closingDeliveriesService = scope.ServiceProvider.GetRequiredService<IClosingDeliveriesService>();				
				var orderWithoutShipmentForDebtPreparer = scope.ServiceProvider.GetRequiredService<IOrderWithoutShipmentForDebtPreparer>();
				var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
				var closingDeliveriesNotificationSender = scope.ServiceProvider.GetRequiredService<IClosingDeliveriesNotificationSender>();

				using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot(nameof(EmailClosingDeliveriesWorker));

				var counterpartiesWithClosingDeliveries = await closingDeliveriesService.CloseDeliveriesForDebtorsAsync(unitOfWork, cancellationToken: stoppingToken);

				_logger.LogInformation("Закрыты поставки {CounterpartiesCount}", counterpartiesWithClosingDeliveries.Count);

				var counterpartiesForClosingDeliveriesMailing = counterpartiesWithClosingDeliveries
					.Where(x => !x.Organization.DisableClosingDeliveriesMailing && !x.Counterparty.DisableClosingDeliveriesMailing)
					.ToList();
				
				if(!counterpartiesForClosingDeliveriesMailing.Any())
				{
					await unitOfWork.CommitAsync(stoppingToken);

					await _zabbixSender.SendIsHealthyAsync(stoppingToken);

					_logger.LogInformation("Нет подходящих для отправки почты контрагентов. Ждём следующего запуска.");

					return;
				}

				_logger.LogInformation("Выбрано {CounterpartiesCount} контрагентов для отправки почты. Подготовка писем для отправки.", counterpartiesForClosingDeliveriesMailing.Count);				

				var notificationInfos = await orderWithoutShipmentForDebtPreparer.PrepareInfo(unitOfWork, counterpartiesForClosingDeliveriesMailing, stoppingToken);				

				await unitOfWork.CommitAsync(stoppingToken);

				_logger.LogInformation("Отправка писем контрагентам.");

				await closingDeliveriesNotificationSender.SendNotifications(unitOfWork, notificationInfos, stoppingToken);

				await unitOfWork.CommitAsync(stoppingToken);				

				await _zabbixSender.SendIsHealthyAsync(stoppingToken);

				_logger.LogInformation("Закрытие поставок контрагентам и отправка почты завершено");
			}
			catch(Exception ex)
			{			
				await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, ex.Message, stoppingToken);

				_logger.LogError(ex, "Ошибка при выполнении закрытия поставок контрагентам и отправке почты");
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

			_logger.LogInformation("Воркер закрытия поставок и отправки почты контрагентам: текущее время {CurrentTime}, час запуска {StartHour}, следующий запуск в {NextRunTime} (через {Delay})", now, startHour, nextRun, delay);

			await Task.Delay(delay, token);
		}
	}
}
