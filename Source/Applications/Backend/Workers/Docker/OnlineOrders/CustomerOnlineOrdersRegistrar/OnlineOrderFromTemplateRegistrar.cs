using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Application.Orders.Services;
using VodovozBusiness.Services.Orders;

namespace CustomerOnlineOrdersRegistrar
{
	public class OnlineOrderFromTemplateRegistrar : BackgroundService
	{
		private readonly ILogger<OnlineOrderFromTemplateRegistrar> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		public OnlineOrderFromTemplateRegistrar(
			ILogger<OnlineOrderFromTemplateRegistrar> logger,
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
				var options = scope.ServiceProvider.GetService<IOptionsMonitor<OnlineOrderFromTemplateRegistrarOptions>>().CurrentValue;
				await Task.Delay(TimeSpan.FromSeconds(options.DelayInSeconds), stoppingToken);
				await TryCreateOnlineOrdersFromTemplates(scope, stoppingToken);
			}
		}

		private async Task TryCreateOnlineOrdersFromTemplates(IServiceScope scope, CancellationToken stoppingToken)
		{
			try
			{
				using var uow = scope.ServiceProvider.GetService<IUnitOfWork>();
				var onlineOrderCreator = scope.ServiceProvider.GetService<IOnlineOrderFromTemplateCreator>();
				await onlineOrderCreator.Create(uow, stoppingToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при создании автозаказов из шаблонов");
			}
		}
	}
}
