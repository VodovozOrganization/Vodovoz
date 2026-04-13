using EdoService.Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EdoService.Library
{
	public static class DependencyGroup
	{
		public static IServiceCollection AddEdoServicesLibrary(this IServiceCollection services)
		{
			services
				.AddScoped<IEdoService, EdoService>()
				.AddScoped<IEdoLogger, EdoLogger>()
				.AddScoped<IAuthorizationService, TaxcomAuthorizationService>()
				;
			
			return services;
		}
	}
}
