using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrueMark.Api.Options;

namespace TrueMark.Api
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureTrueMarkApi(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<TrueMarkApiOptions>(context.Configuration.GetSection(nameof(TrueMarkApiOptions)));
	}
}
