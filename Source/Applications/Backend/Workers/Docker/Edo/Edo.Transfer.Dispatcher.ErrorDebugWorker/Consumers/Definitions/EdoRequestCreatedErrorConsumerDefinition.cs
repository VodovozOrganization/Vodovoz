using MassTransit;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class EdoRequestCreatedErrorConsumerDefinition : ConsumerDefinition<EdoRequestCreatedErrorConsumer>
	{
		public EdoRequestCreatedErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-request-created.consumer.scheduler_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoRequestCreatedErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}

}
