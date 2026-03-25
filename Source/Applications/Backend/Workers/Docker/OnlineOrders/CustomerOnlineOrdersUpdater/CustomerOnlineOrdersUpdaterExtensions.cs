using CustomerOnlineOrdersUpdater.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Osrm;
using QS.DomainModel.UoW;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Services.Logistics;
using VodovozBusiness.Services.Orders;

namespace CustomerOnlineOrdersUpdater
{
	public static class CustomerOnlineOrdersUpdaterExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<CustomerOnlineOrdersUpdaterOptions>(config.GetSection(CustomerOnlineOrdersUpdaterOptions.Path));
			return services;
		}
		
		public static IServiceCollection AddDependenciesGroup(this IServiceCollection services)
		{
			services
				.AddScoped(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot("Обработка онлайн заказов, ожидающих оплату"))
				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
				.AddScoped<IOnlineOrderService, OnlineOrderService>()
				.AddOsrm()
				.AddHostedService<CustomerOnlineOrdersUpdateWorker>()
				;

			return services;
		}
	}
}
