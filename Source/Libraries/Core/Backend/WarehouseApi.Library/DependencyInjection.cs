using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz;
using Vodovoz.FirebaseCloudMessaging;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models;
using WarehouseApi.Library.Converters;
using WarehouseApi.Library.Services;

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
				.AddCore()
				.AddInfrastructure()
				.AddRepositories()
				.AddTrackedUoW()
				.AddFirebaseCloudMessaging(configuration)
				.ConfigureBusinessOptions(configuration)
				.AddScoped<ICarLoadService, CarLoadService>()
				.AddScoped<IRouteListDailyNumberProvider, RouteListDailyNumberProvider>()
				.AddScoped<CarLoadDocumentConverter>();

			services.AddStaticHistoryTracker();

			return services;
		}
	}
}
