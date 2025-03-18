using Edo.Common;
using Edo.Problems;
using Edo.Transfer.Dispatcher.Consumers;
using Edo.Transfer.Dispatcher.Consumers.Definitions;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferDispatcher(this IServiceCollection services)
		{
			services
				.AddScoped<TransferEdoHandler>()
				;

			services
				.AddEdo()
				.AddEdoTransfer()
				.AddEdoProblemRegistation()
				.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot())
				;

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<TransferRequestCreatedConsumer, TransferRequestCreatedConsumerDefinition>();
				cfg.AddConsumer<TransferDocumentAcceptedConsumer, TransferDocumentAcceptedConsumerDefinition>();
			});

			return services;
		}
	}
}
