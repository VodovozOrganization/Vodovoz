using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Transfer.Dispatcher.Consumers.Definitions
{
	public class TransferRequestCreatedConsumerDefinition : ConsumerDefinition<TransferRequestCreatedConsumer>
	{
		public TransferRequestCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-request-created.consumer.transfer-dispatcher");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferRequestCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferRequestCreatedEvent>();
			}
		}
	}
}
