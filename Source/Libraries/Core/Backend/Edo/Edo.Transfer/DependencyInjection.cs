using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Edo.Transfer
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransfer(this IServiceCollection services)
		{
			services.TryAddScoped<TransferDispatcher>();
			services.TryAddScoped<TransferTaskRepository>();

			return services;
		}
	}
}
