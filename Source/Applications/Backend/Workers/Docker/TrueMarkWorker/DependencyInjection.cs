using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrueMarkWorker.Options;

namespace TrueMarkWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureTrueMarkWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<TrueMarkWorkerOptions>(context.Configuration.GetSection(nameof(TrueMarkWorkerOptions)));
	}
}
