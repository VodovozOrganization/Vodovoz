using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class ReceiptCompleteEventConsumerDefinition : ConsumerDefinition<ReceiptCompleteEventConsumer>
	{
		public ReceiptCompleteEventConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-complete.consumer.receipt-dispatcher_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptCompleteEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<ReceiptCompleteEvent>();
			}
		}
	}
}
