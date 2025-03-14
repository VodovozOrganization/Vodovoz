using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Vodovoz.Core.Domain.Validation;

namespace Vodovoz.Core.Domain
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFeatureManagement(this IServiceCollection services)
		{
			ServiceCollectionExtensions.AddFeatureManagement(services);
			services.AddScoped(typeof(IValidationResultFactory<>), typeof(ValidationResultFactory<>));
			return services;
		}
	}
}
