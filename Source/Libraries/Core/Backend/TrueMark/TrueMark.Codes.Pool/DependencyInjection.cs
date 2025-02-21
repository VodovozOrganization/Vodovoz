using Microsoft.Extensions.DependencyInjection;

namespace TrueMark.Codes.Pool
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
