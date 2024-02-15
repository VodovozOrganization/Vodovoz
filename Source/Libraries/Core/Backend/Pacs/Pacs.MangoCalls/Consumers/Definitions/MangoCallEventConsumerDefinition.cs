using Mango.Core.Dto;
using MassTransit;

namespace Pacs.MangoCalls.Consumers.Definitions
{
	public class MangoCallEventConsumerDefinition : ConsumerDefinition<MangoCallEventConsumer>
	{
		public MangoCallEventConsumerDefinition()
		{
			EndpointName = $"pacs.mango.call_event.consumer";
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
