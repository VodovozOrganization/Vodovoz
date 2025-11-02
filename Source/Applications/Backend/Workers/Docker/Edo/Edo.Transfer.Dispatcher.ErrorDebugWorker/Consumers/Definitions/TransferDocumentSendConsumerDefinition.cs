using Edo.ErrorDebugWorker.Consumers;
using MassTransit;

namespace Edo.ErrorDebugWorker.Consumers.Definitions
{
	public class TransferDocumentSendErrorConsumerDefinition : ConsumerDefinition<TransferDocumentSendErrorConsumer>
	{
		public TransferDocumentSendErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-document-send.consumer.docflow_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferDocumentSendErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}
}
