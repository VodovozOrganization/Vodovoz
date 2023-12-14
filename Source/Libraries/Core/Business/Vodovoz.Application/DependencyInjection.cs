using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Application.Services;
using Vodovoz.Application.Services.Logistics;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services) => services
			.AddApplicationServices();

		public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
			.AddScoped<ICounterpartyService, CounterpartyService>()
			.AddScoped<IRouteListService, RouteListService>()
			.AddScoped<IOrderService, OrderService>();
	}
}
