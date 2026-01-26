using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Admin.Consumers.Definitions
{
	public class OrderDocumentCancelledConsumerDefinition : ConsumerDefinition<OrderDocumentCancelledConsumer>
	{
		public OrderDocumentCancelledConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.order-document-cancelled.consumer.admin");
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
