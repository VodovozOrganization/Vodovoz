using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersUpdater.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using VodovozBusiness.Services.Orders;
using Vodovoz.Zabbix.Sender;

namespace CustomerOnlineOrdersUpdater
{
	public class CustomerOnlineOrdersUpdateWorker : BackgroundService
	{
		private readonly ILogger<CustomerOnlineOrdersUpdateWorker> _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public CustomerOnlineOrdersUpdateWorker(
			ILogger<CustomerOnlineOrdersUpdateWorker> logger,
			IServiceScopeFactory scopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				using var scope = _scopeFactory.CreateScope();
				var options = scope.ServiceProvider.GetService<IOptionsMonitor<CustomerOnlineOrdersUpdaterOptions>>().CurrentValue;
				await Task.Delay(TimeSpan.FromSeconds(options.DelayInSeconds), stoppingToken);
				try
				{
					await TryMoveToManualProcessingWaitingForPaymentOnlineOrders(scope, stoppingToken);
					await SendWaitingForPaymentNotification(scope, stoppingToken);
					await _zabbixSender.SendIsHealthyAsync(nameof(CustomerOnlineOrdersUpdateWorker), stoppingToken);
				}
				catch(Exception ex)
				{
					await _zabbixSender.SendProblemMessageAsync(
						nameof(CustomerOnlineOrdersUpdateWorker),
						ZabixSenderMessageType.Problem,
						$"Ошибка при работе воркера по обновлению онлайн заказов и уведомления клиентов об ожидании оплаты: {ex.Message}",
						stoppingToken);
				}
			}
		}

		private async Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders(IServiceScope scope, CancellationToken cancellationToken)
		{
			try
			{
				var unitOfWorkFactory = scope.ServiceProvider.GetService<IUnitOfWorkFactory>();
				using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
				var unPaidOnlineOrderHandler = scope.ServiceProvider.GetService<IUnPaidOnlineOrderHandler>();
				await unPaidOnlineOrderHandler.TryMoveToManualProcessingWaitingForPaymentOnlineOrders(unitOfWork, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при работе воркера по обновлению онлайн заказов");
				throw;
			}
		}

		private async Task SendWaitingForPaymentNotification(IServiceScope scope, CancellationToken cancellationToken)
		{
			try
			{
				var unitOfWorkFactory = scope.ServiceProvider.GetService<IUnitOfWorkFactory>();
				using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
				var unPaidOnlineOrderHandler = scope.ServiceProvider.GetService<IUnPaidOnlineOrderHandler>();
				await unPaidOnlineOrderHandler.SendWaitingForPaymentNotificationsAsync(unitOfWork, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке уведомлений о ожидании оплаты онлайн заказов");
				throw;
			}
		}
	}
}
