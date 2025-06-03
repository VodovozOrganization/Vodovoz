using Edo.Contracts.Messages.Events;
using MassTransit;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class DocumentTransferCompleteErrorConsumerDefinition : ConsumerDefinition<DocumentTransferCompleteErrorConsumer>
	{
		public DocumentTransferCompleteErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-complete.consumer.documents_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<DocumentTransferCompleteErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}

	

}
