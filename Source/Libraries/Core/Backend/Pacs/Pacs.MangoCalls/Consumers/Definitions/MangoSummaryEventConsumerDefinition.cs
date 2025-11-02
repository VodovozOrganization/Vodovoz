using Core.Infrastructure;
using Mango.Core.Dto;
using MassTransit;
using System;

namespace Pacs.MangoCalls.Consumers.Definitions
{
	public class MangoSummaryEventConsumerDefinition : ConsumerDefinition<MangoSummaryEventConsumer>
	{
		public MangoSummaryEventConsumerDefinition()
		{
			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.mango.event.summary.consumer-server";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<MangoSummaryEventConsumer> consumerConfigurator)
		{
			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;

				rmq.Bind<MangoSummaryEvent>();

				rmq.ConcurrentMessageLimit = 1;
				rmq.PrefetchCount = 10;
			}
		}
	}
}
