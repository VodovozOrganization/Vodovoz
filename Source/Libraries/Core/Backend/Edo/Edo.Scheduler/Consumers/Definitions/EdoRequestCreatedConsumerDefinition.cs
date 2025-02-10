using Edo.Contracts.Messages.Events;
using Edo.Transport.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class EdoRequestCreatedConsumerDefinition : ConsumerDefinition<EdoRequestCreatedConsumer>
	{
		public EdoRequestCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.event.request_created.consumer.scheduler");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoRequestCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<EdoRequestCreatedEvent>();
			}
		}
	}
}
