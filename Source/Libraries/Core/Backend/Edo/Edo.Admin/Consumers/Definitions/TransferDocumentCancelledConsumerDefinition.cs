using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Admin.Consumers.Definitions
{
	public class TransferDocumentCancelledConsumerDefinition : ConsumerDefinition<TransferDocumentCancelledConsumer>
	{
		public TransferDocumentCancelledConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-document-cancelled.consumer.admin");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferDocumentCancelledConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferDocumentCancelledEvent>();
			}
		}
	}
}
