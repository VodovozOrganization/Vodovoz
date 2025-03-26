using Edo.Common;
using Edo.Problems;
using Edo.Transfer.Routine;
using Edo.Transfer.Routine.Options;
using Edo.Transfer.Routine.WaitingTransfersUpdate;
using Edo.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;

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

			services.AddWaitingTransfersUpdateWorker();

			return services;
		}

		private static IServiceCollection AddWaitingTransfersUpdateWorker(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureWaitingTransfersUpdateSettings>();

			services
				.AddEdo()
				.AddEdoProblemRegistation()
				.AddHttpClient()
				.AddScoped<TransferEdoHandler>()
				.AddScoped<WaitingTransfersUpdateService>()
				.AddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot(nameof(Routine)));

			services.AddHostedService<WaitingTransfersUpdateWorker>();

			return services;
		}
	}
}
