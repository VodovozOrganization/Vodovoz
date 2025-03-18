using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class ReceiptTaskCreatedEventConsumerDefinition : ConsumerDefinition<ReceiptTaskCreatedEventConsumer>
	{
		public ReceiptTaskCreatedEventConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-task-created.consumer.receipt-dispatcher_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptTaskCreatedEventConsumer> consumerConfigurator)
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
