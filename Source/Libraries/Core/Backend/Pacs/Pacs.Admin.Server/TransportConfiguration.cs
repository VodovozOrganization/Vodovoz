using MassTransit;
using Pacs.Core.Messages.Filters;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Admin.Server
{
	public static class TransportConfiguration
	{
		public static void ConfigureAdminMessagesTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.Message<PacsDomainSettings>(x => x.SetEntityName("pacs.domain_settings.publish"));
		}

		public static void ConfigureAdminMessagesPublishTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.ConfigureAdminMessagesTopology(context);

			cfg.UsePublishFilter(typeof(PublishTimeToLiveFilter<>), context);

			cfg.Publish<PacsDomainSettings>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
