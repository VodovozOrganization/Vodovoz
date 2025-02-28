using Edo.Transfer.Routine;
using Edo.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferRoutine(this IServiceCollection services)
		{
			services
				.AddScoped<StaleTransferSender>()
				;

			services.AddEdoTransfer();

			services.AddHostedService<TransferTimeoutWorker>();

			services.AddEdoMassTransit();

			return services;
		}
	}
}
