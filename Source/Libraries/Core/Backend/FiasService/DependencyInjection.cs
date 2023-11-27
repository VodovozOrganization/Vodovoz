using Fias.Client.Cache;
using Fias.Client.Loaders;
using Microsoft.Extensions.DependencyInjection;

namespace Fias.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDataLoaders(this IServiceCollection services) => services
			.AddScoped<ICitiesDataLoader, CitiesDataLoader>()
			.AddScoped<IStreetsDataLoader, StreetsDataLoader>()
			.AddScoped<IHousesDataLoader, HousesDataLoader>();

		public static IServiceCollection AddFiasClient(this IServiceCollection services) => services
			.AddDataLoaders()
			.AddScoped<IFiasApiClientFactory, FiasApiClientFactory>()
			.AddScoped<GeocoderCache>()
			.AddScoped<IFiasApiClient>(sp => sp.GetService<IFiasApiClientFactory>().CreateClient());
	}
}
