using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;

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

			services.TryAddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot());

			return services;
		}
	}
}
