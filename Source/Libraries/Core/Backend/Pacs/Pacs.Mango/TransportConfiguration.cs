using Mango.Core.Dto;
using MassTransit;
using Pacs.Core.Dto.Calls;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pacs.Messaging
{
    public static class TransportConfiguration
    {
		public static void ConfigureMessageTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Message<CallEvent>(x => x.SetEntityName("pacs.mango.call_event"));

			configurator.Publish<CallEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			//configurator.UsePublishFilter(typeof(MyPublishFilter<>), context);
			configurator.UsePublishFilter(typeof(ValidateOrderStatusFilter<>), context);

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
