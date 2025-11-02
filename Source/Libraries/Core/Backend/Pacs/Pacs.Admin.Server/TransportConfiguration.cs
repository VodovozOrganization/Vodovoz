using MassTransit;
using Pacs.Core;

namespace Pacs.Admin.Server
{
	public static class TransportConfiguration
	{
		public static void AddAdminProducerTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.AddPacsBaseTopology(context);
		}
	}
}
