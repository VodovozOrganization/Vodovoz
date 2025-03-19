using Edo.Transfer.Routine;
using Edo.Transfer.Routine.Options;
using Edo.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferRoutine(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddScoped<StaleTransferSender>()
				;

			services.AddEdoTransfer();

			services.AddHostedService<TransferTimeoutWorker>();

			services.AddEdoMassTransit();

			services.AddWaitingTransfersUpdateWorker(configuration);

			return services;
		}

		private static IServiceCollection AddWaitingTransfersUpdateWorker(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<WaitingTransfersUpdateSettings>(options =>
				configuration.GetSection(nameof(WaitingTransfersUpdateSettings)).Bind(options));

			services.AddScoped<TransferEdoHandler>();

			services.AddHostedService<WaitingTransfersUpdateWorker>();

			return services;
		}
	}
}
