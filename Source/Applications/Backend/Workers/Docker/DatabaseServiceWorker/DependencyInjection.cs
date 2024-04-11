using DatabaseServiceWorker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DatabaseServiceWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureClearFastDeliveryAvailabilityHistoryWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<ClearFastDeliveryAvailabilityHistoryOptions>(context.Configuration.GetSection(nameof(ClearFastDeliveryAvailabilityHistoryOptions)));
	}
}
