using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Fault
{
	public class FaultOrderDocumentSentConsumerDefinition : ConsumerDefinition<FaultOrderDocumentSentConsumer>
	{
		public FaultOrderDocumentSentConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.order-document-sent.consumer.documents_fault");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultOrderDocumentSentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<Fault<OrderDocumentSentEvent>>();
			}
		}
	}
}
