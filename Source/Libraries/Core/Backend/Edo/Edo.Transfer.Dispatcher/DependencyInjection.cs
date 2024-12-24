using Edo.Transfer.Dispatcher.Consumers;
using Edo.Transfer.Dispatcher.Consumers.Definitions;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferDispatcher(this IServiceCollection services)
		{
			services
				.AddScoped<TransferEdoHandler>()
				;

			services.AddEdoTransfer();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<TransferRequestCreatedConsumer, TransferRequestCreatedConsumerDefinition>();
				cfg.AddConsumer<TransferDocumentAcceptedConsumer, TransferDocumentAcceptedConsumerDefinition>();
			});

			return services;
		}
	}
}
