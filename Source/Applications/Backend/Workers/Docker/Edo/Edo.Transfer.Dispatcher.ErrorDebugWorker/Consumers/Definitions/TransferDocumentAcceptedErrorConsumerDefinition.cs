using Edo.Contracts.Messages.Events;
using Edo.Docflow.Consumers;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class TransferDocumentAcceptedErrorConsumerDefinition : ConsumerDefinition<TransferDocumentAcceptedErrorConsumer>
	{
		public TransferDocumentAcceptedErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-document-accepted.consumer.transfer-dispatcher_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferDocumentAcceptedErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}

}
