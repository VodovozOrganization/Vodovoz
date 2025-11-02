using Mango.CallsPublishing;
using MassTransit;
using Pacs.Core;

namespace Pacs.MangoCalls
{
	public static class TransportConfiguration
	{
		public static void AddCallsTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.AddMangoTopology(context);
			cfg.AddPacsBaseTopology(context);
		}
	}
}
