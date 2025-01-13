using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Vodovoz.Core.Domain
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFeatureManagement(this IServiceCollection services)
		{
			ServiceCollectionExtensions.AddFeatureManagement(services);

			return services;
		}
	}
}
