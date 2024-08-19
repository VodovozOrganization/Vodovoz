using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using Vodovoz;
using Vodovoz.FirebaseCloudMessaging;

namespace WarehouseApi.Library
{
	public static class DependencyInjection
	{

		/// <summary>
		/// Добавление сервисов API приложения склада
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddWarehouseApiDependencies(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddScoped((sp) => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("API приложения склада"))
				.AddFirebaseCloudMessaging(configuration)
				.ConfigureBusinessOptions(configuration);

			return services;
		}
	}
}
