using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrueMarkApi.Services;

namespace DatabaseServiceWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureTrueMarkWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<TrueMarkOptions>(context.Configuration.GetSection(nameof(TrueMarkOptions)));

	}
}
