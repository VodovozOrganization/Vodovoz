using MassTransit;
using Pacs.Core;

namespace Pacs.Operators.Break.Server
{
	public static class TransportConfiguration
	{
		public static void AddPacsBreakServerTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.AddPacsBaseTopology(context);
		}
	}
}
