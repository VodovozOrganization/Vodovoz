using Edo.Admin.Consumers;
using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Admin.Consumers.Definitions
{
	public class EdoCancellationConsumerDefinition : ConsumerDefinition<EdoCancellationConsumer>
	{
		public EdoCancellationConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.request-task-cancellation.consumer.admin");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoCancellationConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<RequestTaskCancellationEvent>();
			}
		}
	}
}
