using Edo.Transport2;
using MassTransit;
using RabbitMQ.Client;

namespace TaxcomEdoConsumer.Consumers
{
	public class EdoDocflowUpdatedEventConsumerDefinition : ConsumerDefinition<EdoDocflowUpdatedEventConsumer>
	{
		public EdoDocflowUpdatedEventConsumerDefinition()
		{
			Endpoint(x =>
			{
				x.Name = EdoDocflowUpdatedEvent.Event;
			});
		}
		
		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoDocflowUpdatedEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			
			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.Durable = true;
				rmq.AutoDelete = false;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OutgoingTaxcomDocflowUpdatedEvent>();
			}
		}
	}
}
