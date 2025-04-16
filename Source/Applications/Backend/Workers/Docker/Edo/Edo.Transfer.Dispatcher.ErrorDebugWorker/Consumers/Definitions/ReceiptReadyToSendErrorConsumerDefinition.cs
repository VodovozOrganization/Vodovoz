using Edo.Contracts.Messages.Events;
using MassTransit;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class ReceiptReadyToSendErrorConsumerDefinition : ConsumerDefinition<ReceiptReadyToSendErrorConsumer>
	{
		public ReceiptReadyToSendErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-ready-to-send.consumer.receipt-sender_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptReadyToSendErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}
}
