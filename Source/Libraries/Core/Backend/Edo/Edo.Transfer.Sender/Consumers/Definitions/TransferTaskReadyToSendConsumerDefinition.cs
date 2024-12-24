using Edo.Transfer.Sender.Consumers;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Transfer.Sender.Consumers.Definitions
{
	public class TransferTaskReadyToSendConsumerDefinition : ConsumerDefinition<TransferTaskReadyToSendConsumer>
	{
		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferTaskReadyToSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Exclusive = true;
				rmq.ExchangeType = ExchangeType.Fanout;
			}
		}
	}
}
