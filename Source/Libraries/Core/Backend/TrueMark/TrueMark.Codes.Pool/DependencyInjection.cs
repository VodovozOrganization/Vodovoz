using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;

namespace TrueMark.Codes.Pool
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Расширение для добавления зависимостей работы с пулом кодов
		/// </summary>
		/// <param name="services">Контейнер зависимостей</param>
		/// <returns></returns>
		public static IServiceCollection AddCodesPool(this IServiceCollection services)
		{
			services.TryAddScoped<ITrueMarkCodesPool, TrueMarkCodesPool>();
			services.TryAddScoped<ReceiptTrueMarkCodesPool>();
			services.TryAddScoped<TrueMarkCodesPoolManager>();
			services.TryAddScoped<TrueMarkCodesPoolFactory>();

			services.TryAddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot());

			return services;
		}
	}
}
