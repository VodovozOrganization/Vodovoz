using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Vodovoz.Core.Domain
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCoreDomainServices(this IServiceCollection services)
		{
			services.AddFeatureManagement();

			return services;
		}
	}
}
