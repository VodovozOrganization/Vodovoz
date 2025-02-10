using Microsoft.Extensions.DependencyInjection;
using TrueMarkApi.Client;

namespace Edo.Common
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdo(this IServiceCollection services)
		{
			//services.AddTrueMarkApiClient();

			services
				.AddScoped<TransferRequestCreator>()
				.AddScoped<EdoTaskItemTrueMarkStatusProvider>()
				.AddScoped<EdoTaskItemTrueMarkStatusProviderFactory>()
				;

			return services;
		}
	}
}
