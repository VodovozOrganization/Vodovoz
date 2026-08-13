using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Fault
{
	public class FaultOrderDocumentSendConsumerDefinition : ConsumerDefinition<FaultOrderDocumentSendConsumer>
	{
		public FaultOrderDocumentSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.order-document-send.consumer.docflow_fault");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultOrderDocumentSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<Fault<OrderDocumentSendEvent>>();
			}
		}
	}
}
