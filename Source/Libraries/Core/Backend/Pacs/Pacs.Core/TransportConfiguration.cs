using MassTransit;
using Pacs.Core.Messages.Commands;
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

			configurator.Message<OperatorState>(x => x.SetEntityName("pacs.operator.event.state"));
		}

		public static void ConfigurePublishTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.ConfigureMessageTopology(context);

			configurator.Send<OperatorState>(x => x.UseRoutingKeyFormatter(ctx => ctx.Message.OperatorId.ToString()));

			configurator.Publish<OperatorState>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
