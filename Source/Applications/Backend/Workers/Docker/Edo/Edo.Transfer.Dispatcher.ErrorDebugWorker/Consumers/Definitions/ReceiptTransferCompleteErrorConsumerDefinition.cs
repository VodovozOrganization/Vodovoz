using Edo.Contracts.Messages.Events;
using MassTransit;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class ReceiptTransferCompleteErrorConsumerDefinition : ConsumerDefinition<ReceiptTransferCompleteErrorConsumer>
	{
		public ReceiptTransferCompleteErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-complete.consumer.receipt-dispatcher_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptTransferCompleteErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}
}
