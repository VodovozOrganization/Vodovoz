using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using QS.Utilities.Extensions;
using Vodovoz.Core.Domain.Common;
using VodovozInfrastructure;

namespace Vodovoz.Infrastructure.Persistance
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(
			this IServiceCollection services,
			DependencyType dependencyType = DependencyType.Scoped)
		{
			return services
				.AddService(typeof(IGenericRepository<>), typeof(GenericRepository<>), dependencyType)
				.AddRepositories(dependencyType);
		}

		public static IServiceCollection AddRepositories(
			this IServiceCollection services,
			DependencyType dependencyType = DependencyType.Scoped)
		{
			return services.AddServicesEndsWith(typeof(DependencyInjection).Assembly, "Repository", dependencyType);
		}
	}
}
