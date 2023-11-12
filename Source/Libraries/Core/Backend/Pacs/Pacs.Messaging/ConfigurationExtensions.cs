using MassTransit;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;

namespace Pacs.Messaging
{
	public static class ConfigurationExtensions
    {
		public static void ConfigureMessageTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Message<CallEvent>(x => x.SetEntityName("pacs.call_event"));

			configurator.Publish<CallEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.AutoDelete = true;
			});

			//configurator.UsePublishFilter(typeof(MyPublishFilter<>), context);
			//configurator.UsePublishFilter(typeof(ValidateOrderStatusFilter<>), context);

			/*configurator.ConfigurePublish(x => x.UseExecuteAsync(f => {
                if (f.HasPayloadType(typeof(ClientAvailable)))
                {
                    f.TimeToLive = TimeSpan.FromMinutes(5);
                }
                return Task.CompletedTask;
            }));*/

			//MessageDataDefaults.TimeToLive = TimeSpan.FromMinutes(5);
		}
	}
}
