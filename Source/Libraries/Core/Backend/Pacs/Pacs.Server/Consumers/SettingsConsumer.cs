using Core.Infrastructure;
using MassTransit;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Server;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace Pacs.Server.Consumers
{
	public class PacsSettingsConsumer : IConsumer<SettingsEvent>
	{
		private readonly GlobalBreakController _globalBreakController;

		public PacsSettingsConsumer(GlobalBreakController globalBreakController)
		{
			_globalBreakController = globalBreakController ?? throw new ArgumentNullException(nameof(globalBreakController));
		}

		public async Task Consume(ConsumeContext<SettingsEvent> context)
		{
			_globalBreakController.UpdateSettings(context.Message.Settings);
			await Task.CompletedTask;
		}
	}

	public class PacsSettingsConsumerDefinition : ConsumerDefinition<PacsSettingsConsumer>
	{
		public PacsSettingsConsumerDefinition()
		{
			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.settings.consumer-server";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<PacsSettingsConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<SettingsEvent>();
			}
		}
	}
}
