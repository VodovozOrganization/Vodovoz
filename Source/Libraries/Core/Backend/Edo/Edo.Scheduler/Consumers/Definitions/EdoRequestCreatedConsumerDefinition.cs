using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class EdoRequestCreatedConsumerDefinition : ConsumerDefinition<EdoRequestCreatedConsumer>
	{
		public EdoRequestCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-request-created.consumer.scheduler");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoRequestCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<EdoRequestCreatedEvent>();
			}
		}
	}
}
