using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrueMarkApi.Options;

namespace TrueMarkApi
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureTrueMarkApi(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<TrueMarkApiOptions>(context.Configuration.GetSection(nameof(TrueMarkApiOptions)));
	}
}
