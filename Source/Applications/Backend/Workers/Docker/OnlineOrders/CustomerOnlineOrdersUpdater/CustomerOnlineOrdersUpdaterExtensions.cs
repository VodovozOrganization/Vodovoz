using CustomerOnlineOrdersUpdater.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;

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
				.AddHostedService<CustomerOnlineOrdersUpdateWorker>()
				;

			return services;
		}
	}
}
