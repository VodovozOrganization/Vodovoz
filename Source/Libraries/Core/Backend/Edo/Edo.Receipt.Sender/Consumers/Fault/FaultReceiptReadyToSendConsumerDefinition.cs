using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Sender.Consumers.Fault
{
	public class FaultReceiptReadyToSendConsumerDefinition : ConsumerDefinition<FaultReceiptReadyToSendConsumer>
	{
		public FaultReceiptReadyToSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-ready-to-send.consumer.receipt-sender_fault");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultReceiptReadyToSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<Fault<ReceiptReadyToSendEvent>>();
			}
		}
	}
}
