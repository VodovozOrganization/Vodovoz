using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using ScannedTrueMarkCodesDelayedProcessing.Library.Services;
using VodovozBusiness.Services.TrueMark;

namespace ScannedTrueMarkCodesDelayedProcessing.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddScannedTrueMarkCodesDelayedProcessing(this IServiceCollection services)
		{
			services
				.AddMessageTransportSettings()
				//.AddApplication()
				.AddScoped<IRouteListItemTrueMarkProductCodesProcessingService, RouteListItemTrueMarkProductCodesProcessingService>()
				.AddScoped<ScannedCodesDelayedProcessingService>();

			return services;
		}

	}
}
