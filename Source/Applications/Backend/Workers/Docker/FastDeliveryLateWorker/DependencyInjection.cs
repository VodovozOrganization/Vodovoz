using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FastDeliveryLateWorker
{
	public static class DependencyInjection
	{		
		public static IServiceCollection ConfigureFastDeliveryLateOptions(this IServiceCollection services, HostBuilderContext context) =>
			services.Configure<FastDeliveryLateOptions>(context.Configuration.GetSection(nameof(FastDeliveryLateOptions)));
	}
}
