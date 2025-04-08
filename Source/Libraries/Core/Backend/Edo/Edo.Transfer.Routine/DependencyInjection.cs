using Edo.Common;
using Edo.Problems;
using Edo.Transfer.Routine;
using Edo.Transfer.Routine.Options;
using Edo.Transfer.Routine.WaitingTransfersUpdate;
using Edo.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferRoutineServices(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureWaitingTransfersUpdateSettings>();

			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<StaleTransferSender>();
			services.TryAddScoped<TransferEdoHandler>();
			services.TryAddScoped<WaitingTransfersUpdateService>();

			services
				.AddEdo()
				.AddEdoProblemRegistation()
				.AddHttpClient()
				.AddEdoTransfer()
				;

			return services;
		}

		public static IServiceCollection AddEdoTransferRoutine(this IServiceCollection services)
		{
			services
				.AddEdoMassTransit()
				;

			services
				.AddHostedService<TransferTimeoutWorker>()
				.AddHostedService<WaitingTransfersUpdateWorker>()
				;

			return services;
		}
	}
}
