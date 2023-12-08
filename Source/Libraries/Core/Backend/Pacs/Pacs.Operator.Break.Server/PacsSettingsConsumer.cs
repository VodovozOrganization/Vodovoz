using Core.Infrastructure;
using MassTransit;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace Pacs.Operators.Break.Server
{
	internal class SettingsConsumer : IConsumer<SettingsEvent>
	{
		private readonly GlobalBreakController _breaksController;

		public SettingsConsumer(GlobalBreakController breaksController)
		{
			_breaksController = breaksController ?? throw new System.ArgumentNullException(nameof(breaksController));
		}

		public async Task Consume(ConsumeContext<SettingsEvent> context)
		{
			_breaksController.UpdateSettings(context.Message.Settings);
			await Task.CompletedTask;
		}
	}

	internal class SettingsConsumerDefinition : ConsumerDefinition<SettingsConsumer>
	{
		public SettingsConsumerDefinition()
		{
			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.settings.consumer-break-server";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<SettingsConsumer> consumerConfigurator)
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
