using MassTransit;

namespace Edo.Receipt.Dispatcher.Consumers.Definitions
{
	public class ReceiptTaskCreatedErrorConsumerDefinition : ConsumerDefinition<ReceiptTaskCreatedErrorConsumer>
	{
		public ReceiptTaskCreatedErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-task-created.consumer.receipt-dispatcher_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptTaskCreatedErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}
}
