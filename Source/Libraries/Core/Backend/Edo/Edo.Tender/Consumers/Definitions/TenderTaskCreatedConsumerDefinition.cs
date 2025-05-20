using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Tender.Consumers.Definitions
{
	public class TenderTaskCreatedConsumerDefinition : ConsumerDefinition<TenderTaskCreatedConsumer>
	{
		public TenderTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.tender-task-created.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TenderTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TenderTaskCreatedEvent>();
			}
		}
	}
}
