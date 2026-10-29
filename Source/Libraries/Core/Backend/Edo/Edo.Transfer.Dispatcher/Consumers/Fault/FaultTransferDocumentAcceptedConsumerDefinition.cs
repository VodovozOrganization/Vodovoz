using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Transfer.Dispatcher.Consumers.Fault
{
	public class FaultTransferDocumentAcceptedConsumerDefinition : ConsumerDefinition<FaultTransferDocumentAcceptedConsumer>
	{
		public FaultTransferDocumentAcceptedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-document-accepted.consumer.transfer-dispatcher_fault");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultTransferDocumentAcceptedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<TransferDocumentAcceptedEvent>();
			}
		}
	}
}
