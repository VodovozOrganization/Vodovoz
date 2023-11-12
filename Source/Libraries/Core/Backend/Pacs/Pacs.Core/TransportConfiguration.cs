using MassTransit;
using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core
{
	public static class TransportConfiguration
	{
		public static void ConfigureMessageTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Message<Connect>(x => x.SetEntityName("pacs.operator.command.connect"));
			configurator.Message<Disconnect>(x => x.SetEntityName("pacs.operator.command.disconnect"));
			configurator.Message<StartWorkShift>(x => x.SetEntityName("pacs.operator.command.start_work_shift"));
			configurator.Message<EndWorkShift>(x => x.SetEntityName("pacs.operator.command.end_work_shift"));
			configurator.Message<StartBreak>(x => x.SetEntityName("pacs.operator.command.start_break"));
			configurator.Message<EndBreak>(x => x.SetEntityName("pacs.operator.command.end_break"));
			configurator.Message<ChangePhone>(x => x.SetEntityName("pacs.operator.command.change_phone"));

			configurator.Message<OperatorStateEvent>(x => x.SetEntityName("pacs.operator.event.state"));
			configurator.Message<SettingsEvent>(x => x.SetEntityName("pacs.operator.event.settings"));
		}

		public static void ConfigurePublishTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.ConfigureMessageTopology(context);

			configurator.Send<OperatorStateEvent>(x => x.UseRoutingKeyFormatter(ctx => ctx.Message.OperatorId.ToString()));

			configurator.Publish<OperatorStateEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});

			configurator.Publish<SettingsEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
