using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Transfer.Sender.Consumers.Definitions
{
	public class TransferTaskReadyToSendConsumerDefinition : ConsumerDefinition<TransferTaskReadyToSendConsumer>
	{
		public TransferTaskReadyToSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-task-ready-to-send.consumer.transfer-sender");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferTaskReadyToSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferTaskReadyToSendEvent>();
			}
		}
	}
}
