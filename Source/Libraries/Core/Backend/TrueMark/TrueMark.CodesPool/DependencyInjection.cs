using Microsoft.Extensions.DependencyInjection;

namespace TrueMark.CodesPool
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCodesPool(this IServiceCollection services)
		{
			services
				.AddScoped<TrueMarkCodesPool>()
				.AddScoped<TrueMarkCodesPoolManager>()
				.AddScoped<TrueMarkCodesPoolFactory>()
				;

			return services;
		}
	}
}
