using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class OrderDocumentCancelledConsumerDefinition : ConsumerDefinition<OrderDocumentCancelledConsumer>
	{
		public OrderDocumentCancelledConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-document-cancelled.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OrderDocumentCancelledConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OrderDocumentCancelledEvent>();
			}
		}
	}
}
