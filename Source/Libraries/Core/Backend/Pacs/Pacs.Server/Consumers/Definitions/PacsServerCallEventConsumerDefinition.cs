using MassTransit;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;

namespace Pacs.Server.Consumers.Definitions
{
	public class PacsServerCallEventConsumerDefinition : ConsumerDefinition<PacsServerCallEventConsumer>
	{
		public PacsServerCallEventConsumerDefinition()
		{
			ConcurrentMessageLimit = 1;
			Endpoint(x =>
			{
				x.Name = $"pacs.event.call.consumer-server";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<PacsServerCallEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			endpointConfigurator.PrefetchCount = 64;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<PacsCallEvent>();
			}
		}
	}
}
