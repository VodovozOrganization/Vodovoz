using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Transfer.Sender.Consumers.Definitions
{
	public class TransferTaskPrepareToSendEventConsumerDefinition : ConsumerDefinition<TransferTaskPrepareToSendEventConsumer>
	{
		public TransferTaskPrepareToSendEventConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-task-prepare-to-send.consumer.transfer-sender");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferTaskPrepareToSendEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferTaskPrepareToSendEvent>();
			}
		}
	}
}
