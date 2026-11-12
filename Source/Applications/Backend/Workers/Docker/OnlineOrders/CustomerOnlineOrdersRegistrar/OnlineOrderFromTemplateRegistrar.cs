using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Sale;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Sale;

namespace CustomerOnlineOrdersRegistrar
{
	/// <summary>
	/// Воркер для периодичного создания онлайн заказов из шаблонов 
	/// </summary>
	public class OnlineOrderFromTemplateRegistrar : BackgroundService
	{
		private readonly ILogger<OnlineOrderFromTemplateRegistrar> _logger;
		private readonly IOptionsMonitor<OnlineOrderFromTemplateRegistrarOptions> _templateOptionsMonitor;
		private readonly IServiceScopeFactory _scopeFactory;

		public OnlineOrderFromTemplateRegistrar(
			ILogger<OnlineOrderFromTemplateRegistrar> logger,
			IOptionsMonitor<OnlineOrderFromTemplateRegistrarOptions> templateOptionsMonitor,
			IServiceScopeFactory scopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_templateOptionsMonitor = templateOptionsMonitor ?? throw new ArgumentNullException(nameof(templateOptionsMonitor));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}
		
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				using var scope = _scopeFactory.CreateScope();
				var options = _templateOptionsMonitor.CurrentValue;
				await Task.Delay(TimeSpan.FromSeconds(options.DelayInSeconds), stoppingToken);
				await TryCreateOnlineOrdersFromTemplates(scope, stoppingToken);
			}
		}

		private async Task TryCreateOnlineOrdersFromTemplates(IServiceScope scope, CancellationToken stoppingToken)
		{
			try
			{
				using var uow = scope.ServiceProvider.GetService<IUnitOfWorkFactory>()
					.CreateWithoutRoot("Сервис создания автозаказов из шаблонов");
				var templateCreator = scope.ServiceProvider.GetService<IOnlineOrderTemplateCreator>();
				//await templateCreator.CreateAsync(uow, stoppingToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при создании автозаказов из шаблонов");
			}
		}
	}
}
