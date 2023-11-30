using MassTransit;
using Pacs.Core;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public static class TransportConfiguration
	{
		public static void AddOperatorProducerTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.AddPacsBaseTopology(context);

			cfg.Send<OperatorState>(x => x.UseRoutingKeyFormatter(ctx => $"pacs.operator.state.{ctx.Message.OperatorId}."));
		}
	}
}
