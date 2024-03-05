using Core.Infrastructure;
using MassTransit;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;

namespace Pacs.Server.Consumers.Definitions
{
	public class PacsServerCallEventConsumerDefinition : ConsumerDefinition<PacsServerCallEventConsumer>
	{
		public PacsServerCallEventConsumerDefinition()
		{
			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.call.consumer-server";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<PacsServerCallEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<CallEvent>();
			}
		}
	}
}
