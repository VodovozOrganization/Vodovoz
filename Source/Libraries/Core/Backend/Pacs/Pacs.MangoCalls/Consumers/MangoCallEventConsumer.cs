using Mango.Core.Dto;
using MassTransit;
using Pacs.Core.Messages.Commands;
using Pacs.MangoCalls.Services;
using Pacs.Server.Consumers;
using System;
using System.Threading.Tasks;

namespace Pacs.MangoCalls.Consumers
{
	public class MangoCallEventConsumer : IConsumer<MangoCallEvent>
	{
		private readonly ICallEventRegistrar _callEventRegistrar;

		public MangoCallEventConsumer(ICallEventRegistrar callEventRegistrar)
		{
			_callEventRegistrar = callEventRegistrar ?? throw new ArgumentNullException(nameof(callEventRegistrar));
		}

		public async Task Consume(ConsumeContext<MangoCallEvent> context)
		{
			var callEvent = context.Message;
			await _callEventRegistrar.RegisterCallEvent(callEvent);
			return;
		}
	}

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

				rmq.Bind<MangoCallEvent>();
			}
		}
	}
}
