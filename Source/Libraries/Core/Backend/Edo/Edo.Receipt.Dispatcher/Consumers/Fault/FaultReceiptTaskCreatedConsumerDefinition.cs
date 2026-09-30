using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Dispatcher.Consumers.Fault
{
	public class FaultReceiptTaskCreatedConsumerDefinition : ConsumerDefinition<FaultReceiptTaskCreatedConsumer>
	{
		public FaultReceiptTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-task-created.consumer.receipt-dispatcher_fault");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultReceiptTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<Fault<ReceiptTaskCreatedEvent>>();
			}
		}
	}
}
