using Core.Infrastructure;
using Mango.Core.Dto;
using MassTransit;

namespace Mango.Service.Consumers.Definitions
{
	public class MangoCallEventConsumerDefinition : ConsumerDefinition<MangoCallEventConsumer>
	{
		public MangoCallEventConsumerDefinition()
		{
			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.mango.event.call.consumer-server";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<MangoCallEventConsumer> consumerConfigurator)
		{
			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;

				rmq.Bind<MangoCallEvent>(x =>
					x.RoutingKey = "#"
				);
			}
		}
	}
}
