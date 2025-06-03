using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Sender.Consumers.Definitions
{
	public class ReceiptSendConsumerDefinition : ConsumerDefinition<ReceiptSendConsumer>
	{
		public ReceiptSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-ready-to-send.consumer.receipt-sender");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<ReceiptReadyToSendEvent>();
			}
		}
	}
}
