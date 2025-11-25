using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Scheduler.Consumers.Definitions
{
	public class InformalEdoRequestCreatedConsumerDefinition : ConsumerDefinition<InformalEdoRequestCreatedConsumer>
	{
		public InformalEdoRequestCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.informal-request-created.consumer.scheduler");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<InformalEdoRequestCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<InformalEdoRequestCreatedEvent>();
			}
		}
	}
}
