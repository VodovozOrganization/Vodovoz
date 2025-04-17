using MassTransit;

namespace Edo.ErrorDebugWorker.Consumers.Definitions
{
	public class OrderDocumentSendErrorConsumerDefinition : ConsumerDefinition<OrderDocumentSendErrorConsumer>
	{
		public OrderDocumentSendErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.order-document-send.consumer.docflow_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OrderDocumentSendErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}
}
