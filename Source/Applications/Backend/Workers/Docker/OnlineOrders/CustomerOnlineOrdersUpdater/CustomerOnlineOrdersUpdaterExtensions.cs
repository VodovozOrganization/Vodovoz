using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;

namespace CustomerOnlineOrdersUpdater
{
	public static class CustomerOnlineOrdersUpdaterExtensions
	{
		public static IServiceCollection AddDependenciesGroup(this IServiceCollection services)
		{
			services
				.AddSingleton(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot("Обработка онлайн заказов, ожидающих оплату"))
				;

			return services;
		}
	}
}
