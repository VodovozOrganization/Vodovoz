using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Infrastructure.Persistance
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services)
			=> services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
				.AddRepositories();

		public static IServiceCollection AddRepositories(this IServiceCollection services)
		{
			var repositoryTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.Name.EndsWith("Repository"));

			foreach(var repositoryType in repositoryTypes)
			{
				var repositoryInterface = repositoryType.GetInterfaces().FirstOrDefault(i => i.Name == $"I{repositoryType.Name}");
				if(repositoryInterface != null)
				{
					services.AddScoped(repositoryInterface, repositoryType);
				}
			}

			return services;
		}
	}
}
