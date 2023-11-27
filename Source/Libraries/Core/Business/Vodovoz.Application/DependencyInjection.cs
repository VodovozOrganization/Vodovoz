using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application.Services;
using Vodovoz.Application.Services.Logistics;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			services.AddScoped<ICounterpartyService, CounterpartyService>();
			services.AddScoped<IRouteListService, RouteListService>();

			return services;
		}
	}
}
