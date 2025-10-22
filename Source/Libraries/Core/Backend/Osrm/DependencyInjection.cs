using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.Osrm;

namespace Osrm
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddOsrm(this IServiceCollection services)
		{
			services.TryAddScoped<IOsrmClient, OsrmClient>();

			return services;
		}
	}
}
