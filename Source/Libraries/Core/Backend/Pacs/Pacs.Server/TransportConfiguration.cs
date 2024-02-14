using MassTransit;
using Pacs.Core;
using Pacs.Core.Messages.Events;

namespace Pacs.Server
{
	public static class TransportConfiguration
	{
		public static void AddOperatorProducerTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.AddPacsBaseTopology(context);

			cfg.Send<OperatorStateEvent>(x => x.UseRoutingKeyFormatter(ctx => $"pacs.operator.state.{ctx.Message.State.OperatorId}."));
		}
	}
}
