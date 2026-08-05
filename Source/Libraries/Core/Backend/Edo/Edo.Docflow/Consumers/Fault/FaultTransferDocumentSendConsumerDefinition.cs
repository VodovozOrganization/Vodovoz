using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Fault
{
	public class FaultTransferDocumentSendConsumerDefinition : ConsumerDefinition<FaultTransferDocumentSendConsumer>
	{
		public FaultTransferDocumentSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-document-send.consumer.docflow_fault");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultTransferDocumentSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<Fault<TransferDocumentSendEvent>>();
			}
		}
	}
}
