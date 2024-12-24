using Edo.Transfer.Sender;
using Edo.Transfer.Sender.Consumers;
using Edo.Transfer.Sender.Consumers.Definitions;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferSender(this IServiceCollection services)
		{
			services
				.AddScoped<TransferSendHandler>()
				;

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<TransferTaskReadyToSendConsumer, TransferTaskReadyToSendConsumerDefinition>();
			});

			return services;
		}
	}
}
