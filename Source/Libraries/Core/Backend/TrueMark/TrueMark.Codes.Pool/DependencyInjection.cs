using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;

namespace TrueMark.Codes.Pool
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCodesPool(this IServiceCollection services)
		{
			services.TryAddScoped<TrueMarkCodesPool>();
			services.TryAddScoped<TrueMarkCodesPoolManager>();
			services.TryAddScoped<TrueMarkCodesPoolFactory>();

			services.TryAddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot());

			return services;
		}
	}
}
