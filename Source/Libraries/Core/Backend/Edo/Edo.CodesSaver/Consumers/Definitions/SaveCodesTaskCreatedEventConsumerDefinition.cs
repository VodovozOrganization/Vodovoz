using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.CodesSaver.Consumers.Definitions
{
	public class SaveCodesTaskCreatedEventConsumerDefinition : ConsumerDefinition<SaveCodesTaskCreatedEventConsumer>
	{
		public SaveCodesTaskCreatedEventConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.codes-save-task-created.consumer.codes-saver");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<SaveCodesTaskCreatedEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				//rmq.AutoDelete = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<SaveCodesTaskCreatedEvent>();
			}
		}
	}
}
