using Mango.CallsPublishing;
using MassTransit;
using Pacs.Core.Messages.Events;
using Pacs.Core.Messages.Filters;
using RabbitMQ.Client;

namespace Pacs.MangoCalls
{
	public static class TransportConfiguration
	{
		public static void ConfigureCallsTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.ConfigureMangoMessageTopology(context);
			cfg.Message<CallEvent>(x => x.SetEntityName("pacs.call_event.publish"));

			cfg.UsePublishFilter(typeof(PublishTimeToLiveFilter<>), context);

			cfg.Publish<CallEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
