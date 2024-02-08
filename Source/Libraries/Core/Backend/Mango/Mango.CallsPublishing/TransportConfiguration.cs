using Mango.Core.Dto;
using MassTransit;
using RabbitMQ.Client;

namespace Mango.CallsPublishing
{
    public static class TransportConfiguration
    {
		/// <summary>
		/// Конфигурирует общие настройки для сообщений
		/// </summary>
		public static void ConfigureMangoMessageTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Message<CallEvent>(x => x.SetEntityName("mango.call_event"));
			configurator.Send<CallEvent>(x => x.UseRoutingKeyFormatter(ctx => $"acdgroup-{ctx.Message.To.AcdGroup}."));
		}

		/// <summary>
		/// Конфигурирует настройки отправки сообщений
		/// </summary>
		public static void ConfigureMangoProducerTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.ConfigureMangoMessageTopology(context);

			configurator.Publish<CallEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Topic;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}


}
