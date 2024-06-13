using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Infrastructure.Persistance
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services)
			=> services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
	}
}
