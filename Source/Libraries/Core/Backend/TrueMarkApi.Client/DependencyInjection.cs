using Microsoft.Extensions.DependencyInjection;

namespace TrueMarkApi.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddTrueMarkApiClient(this IServiceCollection services)
		{
			services
				.AddHttpClient()
				.AddScoped<ITrueMarkApiClientFactory, TrueMarkApiClientFactory>()
				.AddScoped<ITrueMarkApiClient>(sp => sp.GetRequiredService<ITrueMarkApiClientFactory>().GetClient())
				;

			return services;
		}
	}
}
