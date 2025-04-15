using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Dispatcher.Consumers.Definitions
{
	public class ReceiptCompleteConsumerDefinition : ConsumerDefinition<ReceiptCompleteConsumer>
	{
		public ReceiptCompleteConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-complete.consumer.receipt-dispatcher");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptCompleteConsumer> consumerConfigurator)
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
