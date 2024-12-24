using Microsoft.Extensions.DependencyInjection;

namespace Edo.Transfer
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransfer(this IServiceCollection services)
		{
			services
				.AddScoped<TransferDispatcher>()
				.AddScoped<TransferTaskRepository>()
				;

			return services;
		}
	}
}
