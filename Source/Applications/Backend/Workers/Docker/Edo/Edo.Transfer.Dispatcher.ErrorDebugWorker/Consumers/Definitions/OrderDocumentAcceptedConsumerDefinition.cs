using MassTransit;

namespace Edo.ErrorDebugWorker.Consumers.Definitions
{
	public class OrderDocumentAcceptedErrorConsumerDefinition : ConsumerDefinition<OrderDocumentAcceptedErrorConsumer>
	{
		public OrderDocumentAcceptedErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-document-accepted.consumer.documents_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OrderDocumentAcceptedErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}
}
