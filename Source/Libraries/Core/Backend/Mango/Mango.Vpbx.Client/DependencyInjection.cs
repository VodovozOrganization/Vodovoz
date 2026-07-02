using Mango.Vpbx.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Mango.Vpbx.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMangoVpbxClientServices(this IServiceCollection services)
		{
			services
				.AddScoped<IMangoCallsService, MangoCallsService>();

			return services;
		}
	}
}
