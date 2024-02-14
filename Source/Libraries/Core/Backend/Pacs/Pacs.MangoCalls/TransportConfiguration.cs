using Mango.CallsPublishing;
using MassTransit;
using Pacs.Core;

namespace Pacs.MangoCalls
{
	public static class TransportConfiguration
	{
		public static void AddCallsBaseTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.AddPacsBaseTopology(context);
		}

		public static void AddCallsProducerTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.AddMangoBaseTopology(context);

			cfg.AddCallsBaseTopology(context);
		}
	}
}
