using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Logistics.RouteOptimization;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Application.Payments;
using Vodovoz.Application.Services;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services) => services
			.AddScoped<IRouteOptimizer, RouteOptimizer>()
			.AddApplicationServices();

		public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
			.AddScoped<ICounterpartyService, CounterpartyService>()
			.AddScoped<IRouteListService, RouteListService>()
			.AddScoped<PaymentService>()
			.AddScoped<IOrderService, OrderService>();
	}
}
