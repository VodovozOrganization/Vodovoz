using Edo.Common;
using Edo.Problems;
using Edo.Transfer.Routine;
using Edo.Transfer.Routine.Options;
using Edo.Transfer.Routine.Services;
using Edo.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.NHibernate.Repositories.Orders;
using Vodovoz.Core.Data.Repositories;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferRoutineServices(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureWaitingTransfersUpdateSettings>();
			services.ConfigureOptions<ConfigureClosingDocumentsOrdersUpdSendSettings>();

			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<StaleTransferSender>();
			services.TryAddScoped<TransferEdoHandler>();
			services.TryAddScoped<WaitingTransfersUpdateService>();
			services.TryAddScoped<ClosingDocumentsOrdersUpdSendService>();
			services.TryAddScoped<ClosingDocumentsOrdersEquipmentTransferSendService>();

			services.AddScoped<IOrderRepository, OrderRepository>();

			services
				.AddEdo()
				.AddEdoProblemRegistration()
				.AddHttpClient()
				.AddEdoTransfer()
				;

			return services;
		}

		public static IServiceCollection AddEdoTransferRoutine(this IServiceCollection services)
		{
			services
				.AddEdoMassTransit()
				.AddEdoTransferRoutineServices()
				;

			services
				.AddHostedService<TransferTimeoutWorker>()
				.AddHostedService<WaitingTransfersUpdateWorker>()
				.AddHostedService<ClosingDocumentsOrdersUpdSendWorker>()
				.AddHostedService<ClosingDocumentsOrdersEquipmentTransferSendWorker>()
				;

			return services;
		}
	}
}
