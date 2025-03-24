using Edo.Contracts.Messages.Events;
using MassTransit;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class TransferCompleteErrorConsumerDefinition : ConsumerDefinition<TransferCompleteErrorConsumer>
	{
		public TransferCompleteErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-complete.consumer.receipt-dispatcher_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferCompleteErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
			rmq.Batch<TransferCompleteEvent>(x =>
			{
				x.MessageLimit = 1;
			});
		}
	}
	
}
