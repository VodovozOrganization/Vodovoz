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

namespace CustomerOnlineOrdersUpdater
{
	public class CustomerOnlineOrdersUpdateWorker : BackgroundService
	{
		private readonly ILogger<CustomerOnlineOrdersUpdateWorker> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		public CustomerOnlineOrdersUpdateWorker(
			ILogger<CustomerOnlineOrdersUpdateWorker> logger,
			IServiceScopeFactory scopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				using var scope = _scopeFactory.CreateScope();
				var options = scope.ServiceProvider.GetService<IOptionsMonitor<CustomerOnlineOrdersUpdaterOptions>>().CurrentValue;
				await Task.Delay(TimeSpan.FromSeconds(options.DelayInSeconds), stoppingToken);
				await TryMoveToManualProcessingWaitingForPaymentOnlineOrders(scope);
			}
		}

		private async Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders(IServiceScope scope)
		{
			try
			{
				using var uow = scope.ServiceProvider.GetService<IUnitOfWork>();
				var unPaidOnlineOrderHandler = scope.ServiceProvider.GetService<IUnPaidOnlineOrderHandler>();
				await unPaidOnlineOrderHandler.TryMoveToManualProcessingWaitingForPaymentOnlineOrders(uow);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при работе воркера по обновлению онлайн заказов");
			}
		}
	}
}
