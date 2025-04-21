using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrueMarkApi.Client;

namespace Edo.Common
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdo(this IServiceCollection services)
		{
			services.AddTrueMarkApiClient();

			services.TryAddScoped<TransferRequestCreator>();
			services.TryAddScoped<EdoTaskItemTrueMarkStatusProvider>();
			services.TryAddScoped<EdoTaskItemTrueMarkStatusProviderFactory>();
			services.TryAddScoped<ITrueMarkCodesValidator, TrueMarkTaskCodesValidator>();
			services.TryAddScoped<IEdoOrderContactProvider, EdoOrderContactProvider>();

			return services;
		}
	}
}
