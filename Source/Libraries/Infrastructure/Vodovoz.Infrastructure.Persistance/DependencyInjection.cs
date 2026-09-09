using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.Utilities.Extensions;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance.Counterparties;

namespace Vodovoz.Infrastructure.Persistance
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(
			this IServiceCollection services,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			return services
				.AddService(typeof(IGenericRepository<>), typeof(GenericRepository<>), serviceLifetime)
				.AddRepositories(serviceLifetime);
		}

		public static IServiceCollection AddRepositories(
			this IServiceCollection services,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartable, DeliveryPointOrderFrequencyTrackerFactory>());

			return services.AddServicesEndsWith(typeof(DependencyInjection).Assembly, "Repository", serviceLifetime);
		}
	}
}
