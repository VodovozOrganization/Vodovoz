using Microsoft.Extensions.DependencyInjection;

namespace ModulKassa
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddModulKassa(this IServiceCollection services)
		{
			services.AddSingleton<CashboxClientProvider>();

			return services;
		}
	}
}
