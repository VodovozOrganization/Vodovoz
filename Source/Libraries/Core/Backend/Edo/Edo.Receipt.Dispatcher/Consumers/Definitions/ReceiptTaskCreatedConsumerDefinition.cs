using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Dispatcher.Consumers.Definitions
{
	public class ReceiptTaskCreatedConsumerDefinition : ConsumerDefinition<ReceiptTaskCreatedConsumer>
	{
		public ReceiptTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-task-created.consumer.receipt-dispatcher");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<ReceiptTaskCreatedEvent>();
			}
		}
	}
}
