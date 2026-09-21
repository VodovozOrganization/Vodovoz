using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;
using Vodovoz.Core.Domain.Rules.Edo;
using Vodovoz.Core.Domain.Validation;

namespace Vodovoz.Core.Domain
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFeatureManagement(this IServiceCollection services)
		{
			ServiceCollectionExtensions.AddFeatureManagement(services);
			services
				.AddScoped(typeof(IValidationResultFactory<>), typeof(ValidationResultFactory<>))
				.TryAddScoped<CanProcessOrSendEdoByClosingAccountingDate>()
				;
			
			return services;
		}
	}
}
