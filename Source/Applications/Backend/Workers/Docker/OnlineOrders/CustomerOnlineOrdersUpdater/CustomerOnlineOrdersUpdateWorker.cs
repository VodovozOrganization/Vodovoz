using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Core.Domain.Interfaces.Orders;

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
				await Task.Delay(1000, stoppingToken);
				await TryMoveToManualProcessingWaitingForPaymentOnlineOrders();
			}
		}

		private async Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders()
		{
			using var scope = _scopeFactory.CreateScope();
			var unPaidOnlineOrderHandler = scope.ServiceProvider.GetService<IUnPaidOnlineOrderHandler>();
			await unPaidOnlineOrderHandler.TryMoveToManualProcessingWaitingForPaymentOnlineOrders();
		}
	}
}
